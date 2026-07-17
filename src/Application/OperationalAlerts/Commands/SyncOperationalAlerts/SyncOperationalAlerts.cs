using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertRules;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;

public sealed record SyncOperationalAlertsCommand : IRequest;

public sealed class SyncOperationalAlertsCommandHandler : IRequestHandler<SyncOperationalAlertsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public SyncOperationalAlertsCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task Handle(SyncOperationalAlertsCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var now = _timeProvider.GetUtcNow();
        var rules = await Rules(organizationId, cancellationToken);

        var atRiskDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Status == DonorStatus.AtRisk, cancellationToken);
        await UpsertAggregateAlert(
            organizationId,
            rules["DonorRisk"],
            "Doadores em risco exigem retorno",
            $"{atRiskDonors} doadores estao marcados como em risco.",
            atRiskDonors,
            "/doadores?segment=AtRisk",
            now,
            cancellationToken);

        var overdueTasks = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.DueAtUtc != null &&
            task.DueAtUtc < now &&
            task.Status != RelationshipTaskStatus.Completed &&
            task.Status != RelationshipTaskStatus.Cancelled,
            cancellationToken);
        await UpsertAggregateAlert(
            organizationId,
            rules["OverdueTasks"],
            "Tarefas vencidas acumuladas",
            $"{overdueTasks} tarefas estao vencidas e ainda abertas.",
            overdueTasks,
            "/tarefas?due=Overdue",
            now,
            cancellationToken);

        var pendingReceipts = await _context.Donations.AsNoTracking().CountAsync(donation =>
            donation.Status == DonationStatus.Confirmed &&
            !_context.Receipts.Any(receipt => receipt.DonationId == donation.Id && receipt.Status != ReceiptStatus.Cancelled),
            cancellationToken);
        await UpsertAggregateAlert(
            organizationId,
            rules["PendingReceipts"],
            "Recibos pendentes de emissao",
            $"{pendingReceipts} doacoes confirmadas ainda nao possuem recibo valido.",
            pendingReceipts,
            "/recibos?status=Pending",
            now,
            cancellationToken);

        await SyncLowConversionAlerts(organizationId, rules["CampaignLowConversion"], "campaign", now, cancellationToken);
        await SyncLowConversionAlerts(organizationId, rules["ProjectLowConversion"], "project", now, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncLowConversionAlerts(
        Guid organizationId,
        OperationalAlertRule rule,
        string targetType,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var start = now.AddDays(-30);
        var pages = await LandingPages(targetType, cancellationToken);
        var views = await _context.LandingPageViews
            .AsNoTracking()
            .Where(view => view.TargetType == targetType && view.ViewedAtUtc >= start)
            .GroupBy(view => view.TargetId)
            .Select(group => new { TargetId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.TargetId, item => item.Count, cancellationToken);
        var leads = await _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry =>
                entry.RelatedEntityType == targetType &&
                entry.RelatedEntityId != null &&
                entry.Title == "Interesse pela landing page" &&
                entry.OccurredAtUtc >= start)
            .GroupBy(entry => entry.RelatedEntityId!.Value)
            .Select(group => new { TargetId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.TargetId, item => item.Count, cancellationToken);

        foreach (var page in pages)
        {
            var viewsCount = views.GetValueOrDefault(page.TargetId);
            var leadsCount = leads.GetValueOrDefault(page.TargetId);
            var conversion = viewsCount <= 0 ? 0 : Math.Round(leadsCount * 100m / viewsCount, 2);
            var lowConversionThreshold = rule.LowConversionThresholdPercent ?? 5;
            var shouldAlert = rule.ShouldAlert(viewsCount) && conversion < lowConversionThreshold;
            var source = rule.Source;
            var alert = await _context.OperationalAlerts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(entity =>
                    !entity.IsDeleted &&
                    entity.OrganizationId == organizationId &&
                    entity.Source == source &&
                    entity.RelatedEntityType == targetType &&
                    entity.RelatedEntityId == page.TargetId &&
                    entity.Status != OperationalAlertStatus.Resolved,
                    cancellationToken);

            if (!shouldAlert)
            {
                if (alert is not null)
                {
                    alert.Resolve(null, "Resolvido automaticamente porque a conversao saiu da zona de alerta.", now);
                }

                continue;
            }

            var description = $"{page.Name} teve {viewsCount} visitas e {leadsCount} leads nos ultimos 30 dias ({conversion}% de conversao; limite configurado: {lowConversionThreshold}%).";
            if (alert is null)
            {
                _context.OperationalAlerts.Add(OperationalAlert.Create(
                    organizationId,
                    targetType == "campaign" ? "Campanha com baixa conversao" : "Projeto com baixa conversao",
                    description,
                    rule.SeverityFor(viewsCount),
                    source,
                    targetType,
                    page.TargetId,
                    $"/captacao/desempenho?targetType={targetType}",
                    rule.AssignedUserId,
                    now.AddHours(rule.DueInHours),
                    now));
                continue;
            }

            alert.Refresh(description, rule.SeverityFor(viewsCount), alert.AssignedUserId ?? rule.AssignedUserId, alert.DueAtUtc ?? now.AddHours(rule.DueInHours), now);
        }
    }

    private async Task<IReadOnlyCollection<LandingTarget>> LandingPages(string targetType, CancellationToken cancellationToken)
    {
        if (targetType == "campaign")
        {
            return await _context.LandingPages
                .AsNoTracking()
                .Where(page => page.TargetType == "campaign" && page.IsActive && page.IsPublished)
                .Join(_context.Campaigns.AsNoTracking(), page => page.TargetId, campaign => campaign.Id, (page, campaign) => new LandingTarget(page.TargetId, campaign.Name))
                .ToArrayAsync(cancellationToken);
        }

        return await _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == "project" && page.IsActive && page.IsPublished)
            .Join(_context.Projects.AsNoTracking(), page => page.TargetId, project => project.Id, (page, project) => new LandingTarget(page.TargetId, project.Name))
            .ToArrayAsync(cancellationToken);
    }

    private async Task<Dictionary<string, OperationalAlertRule>> Rules(Guid organizationId, CancellationToken cancellationToken)
    {
        var defaults = GetOperationalAlertRulesQueryHandler.DefaultRules(organizationId).ToDictionary(rule => rule.Source);
        var stored = await _context.OperationalAlertRules.ToArrayAsync(cancellationToken);
        foreach (var rule in stored)
        {
            defaults[rule.Source] = rule;
        }

        return defaults;
    }

    private async Task UpsertAggregateAlert(
        Guid organizationId,
        OperationalAlertRule rule,
        string title,
        string description,
        int count,
        string actionUrl,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var source = rule.Source;
        var alert = await _context.OperationalAlerts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(entity =>
                !entity.IsDeleted &&
                entity.OrganizationId == organizationId &&
                entity.Source == source &&
                entity.RelatedEntityType == "OperationalAggregate" &&
                entity.RelatedEntityId == null &&
                entity.Status != OperationalAlertStatus.Resolved,
                cancellationToken);

        if (!rule.ShouldAlert(count))
        {
            if (alert is not null)
            {
                alert.Resolve(null, rule.IsEnabled ? "Resolvido automaticamente porque a condicao deixou de existir." : "Resolvido automaticamente porque a regra foi desativada.", now);
            }

            return;
        }

        if (alert is null)
        {
            _context.OperationalAlerts.Add(OperationalAlert.Create(
                organizationId,
                title,
                description,
                rule.SeverityFor(count),
                source,
                "OperationalAggregate",
                null,
                actionUrl,
                rule.AssignedUserId,
                now.AddHours(rule.DueInHours),
                now));
            return;
        }

        alert.Refresh(description, rule.SeverityFor(count), alert.AssignedUserId ?? rule.AssignedUserId, alert.DueAtUtc ?? now.AddHours(rule.DueInHours), now);
    }

    private sealed record LandingTarget(Guid TargetId, string Name);
}
