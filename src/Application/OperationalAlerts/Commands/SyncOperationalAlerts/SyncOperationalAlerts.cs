using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
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

        var atRiskDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Status == DonorStatus.AtRisk, cancellationToken);
        await UpsertAggregateAlert(
            organizationId,
            "DonorRisk",
            "Doadores em risco exigem retorno",
            $"{atRiskDonors} doadores estao marcados como em risco.",
            atRiskDonors,
            OperationalAlertSeverity.Warning,
            "/doadores?segment=AtRisk",
            now.AddDays(1),
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
            "OverdueTasks",
            "Tarefas vencidas acumuladas",
            $"{overdueTasks} tarefas estao vencidas e ainda abertas.",
            overdueTasks,
            overdueTasks >= 10 ? OperationalAlertSeverity.High : OperationalAlertSeverity.Warning,
            "/tarefas?due=Overdue",
            now.AddHours(8),
            now,
            cancellationToken);

        var pendingReceipts = await _context.Donations.AsNoTracking().CountAsync(donation =>
            donation.Status == DonationStatus.Confirmed &&
            !_context.Receipts.Any(receipt => receipt.DonationId == donation.Id && receipt.Status != ReceiptStatus.Cancelled),
            cancellationToken);
        await UpsertAggregateAlert(
            organizationId,
            "PendingReceipts",
            "Recibos pendentes de emissao",
            $"{pendingReceipts} doacoes confirmadas ainda nao possuem recibo valido.",
            pendingReceipts,
            pendingReceipts >= 10 ? OperationalAlertSeverity.High : OperationalAlertSeverity.Warning,
            "/recibos?status=Pending",
            now.AddDays(2),
            now,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAggregateAlert(
        Guid organizationId,
        string source,
        string title,
        string description,
        int count,
        OperationalAlertSeverity severity,
        string actionUrl,
        DateTimeOffset dueAtUtc,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
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

        if (count <= 0)
        {
            if (alert is not null)
            {
                alert.Resolve(null, "Resolvido automaticamente porque a condicao deixou de existir.", now);
            }

            return;
        }

        if (alert is null)
        {
            _context.OperationalAlerts.Add(OperationalAlert.Create(
                organizationId,
                title,
                description,
                severity,
                source,
                "OperationalAggregate",
                null,
                actionUrl,
                null,
                dueAtUtc,
                now));
            return;
        }

        alert.Refresh(description, severity, alert.AssignedUserId, alert.DueAtUtc ?? dueAtUtc, now);
    }
}
