using System.Globalization;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Dashboard.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Dashboard.Queries.GetDashboardOverview;

public record GetDashboardOverviewQuery : IRequest<DashboardOverviewDto>
{
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public Guid? ProjectId { get; init; }
}

public sealed class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;
    private readonly ISender _sender;

    public GetDashboardOverviewQueryHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider,
        ISender sender)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
        _sender = sender;
    }

    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        await _sender.Send(new SyncOperationalAlertsCommand(), cancellationToken);
        var today = _timeProvider.GetUtcNow();
        var start = request.StartDateUtc ?? new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = request.EndDateUtc ?? start.AddMonths(1).AddTicks(-1);

        var monthlyGoal = await _context.Organizations
            .AsNoTracking()
            .Where(organization => organization.Id == organizationId)
            .Select(organization => organization.DefaultMonthlyGoal ?? 0)
            .FirstOrDefaultAsync(cancellationToken);

        var confirmedDonationQuery = _context.Donations
            .AsNoTracking()
            .Where(donation =>
                donation.Status == DonationStatus.Confirmed &&
                donation.PaidAtUtc != null &&
                donation.PaidAtUtc >= start &&
                donation.PaidAtUtc <= end);

        if (request.ProjectId is not null)
        {
            confirmedDonationQuery = confirmedDonationQuery.Where(donation =>
                _context.DonationProjects.Any(projectLink =>
                    projectLink.DonationId == donation.Id &&
                    projectLink.ProjectId == request.ProjectId));
        }

        var confirmedDonations = await confirmedDonationQuery
            .Select(donation => new
            {
                donation.Id,
                donation.Amount,
                PaidAt = donation.PaidAtUtc!.Value,
                DonorName = donation.Donor.FullName,
                CampaignName = donation.Campaign == null ? "Sem campanha" : donation.Campaign.Name,
                GoalAmount = donation.Campaign == null ? monthlyGoal : donation.Campaign.GoalAmount ?? 0,
                ProjectName = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == donation.Id)
                    .Select(projectLink => projectLink.Project.Name)
                    .FirstOrDefault() ?? "Sem projeto/destinacao",
                ProjectGoalAmount = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == donation.Id)
                    .Select(projectLink => projectLink.Project.GoalAmount ?? 0)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var totalDonors = await _context.Donors.AsNoTracking().CountAsync(cancellationToken);
        var activeDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Status == DonorStatus.Active, cancellationToken);
        var pendingOrOverdue = await _context.Donations.AsNoTracking().CountAsync(donation =>
            donation.Status == DonationStatus.Pending || donation.Status == DonationStatus.Overdue, cancellationToken);
        var dueToday = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.DueAtUtc != null &&
            task.DueAtUtc.Value.Date == today.Date &&
            task.Status != RelationshipTaskStatus.Completed &&
            task.Status != RelationshipTaskStatus.Cancelled, cancellationToken);
        var overdueTasks = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.DueAtUtc != null &&
            task.DueAtUtc.Value.Date < today.Date &&
            task.Status != RelationshipTaskStatus.Completed &&
            task.Status != RelationshipTaskStatus.Cancelled, cancellationToken);
        var atRiskDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Status == DonorStatus.AtRisk, cancellationToken);
        var leadsWithoutDonation = await _context.Donors.AsNoTracking().CountAsync(donor =>
            donor.Status == DonorStatus.Lead && !_context.Donations.Any(donation => donation.DonorId == donor.Id), cancellationToken);
        var missingDocuments = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Document == null || donor.Document == string.Empty, cancellationToken);

        var confirmedAmount = confirmedDonations.Sum(donation => donation.Amount);
        var averageDonation = confirmedDonations.Count == 0 ? 0 : confirmedAmount / confirmedDonations.Count;

        var contactedDonors = await _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry => entry.Type == TimelineEntryType.Contact)
            .Select(entry => entry.DonorId)
            .Distinct()
            .CountAsync(cancellationToken);
        var donorsWithDonation = await _context.Donations
            .AsNoTracking()
            .Select(donation => donation.DonorId)
            .Distinct()
            .CountAsync(cancellationToken);
        var recurringDonors = await _context.DonationPlans
            .AsNoTracking()
            .Where(plan => plan.Status == DonationPlanStatus.Active)
            .Select(plan => plan.DonorId)
            .Distinct()
            .CountAsync(cancellationToken);
        var consentOk = totalDonors == 0 ? 0 : (int)Math.Round(
            await _context.Donors.AsNoTracking().CountAsync(donor => donor.AllowsCommunication && !donor.DoNotContact, cancellationToken) * 100m / totalDonors);
        var openTasks = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.Status == RelationshipTaskStatus.Open || task.Status == RelationshipTaskStatus.InProgress, cancellationToken);
        var openOperationalAlerts = await _context.OperationalAlerts.AsNoTracking().CountAsync(alert =>
            alert.Status != OperationalAlertStatus.Resolved, cancellationToken);
        var highOperationalAlerts = await _context.OperationalAlerts.AsNoTracking().CountAsync(alert =>
            alert.Status != OperationalAlertStatus.Resolved &&
            (alert.Severity == OperationalAlertSeverity.High || alert.Severity == OperationalAlertSeverity.Critical), cancellationToken);

        return new DashboardOverviewDto
        {
            Metrics =
            [
                new("Arrecadacao confirmada", FormatCurrency(confirmedAmount), PeriodLabel(start, end, today), monthlyGoal <= 0 ? "Meta não configurada" : $"{Math.Round(confirmedAmount * 100 / monthlyGoal)}% da meta"),
                new("Doadores ativos", activeDonors.ToString(CultureInfo.InvariantCulture), "Status Active", $"{totalDonors} cadastrados"),
                new("Pendencias", pendingOrOverdue.ToString(CultureInfo.InvariantCulture), "Cobranças pendentes ou vencidas", $"{dueToday} tarefas hoje"),
                new("Ticket medio", FormatCurrency(averageDonation), "Doacoes confirmadas", $"{confirmedDonations.Count} confirmadas"),
            ],
            DonationsByDay = confirmedDonations
                .GroupBy(donation => donation.PaidAt.ToString("dd", CultureInfo.InvariantCulture))
                .OrderBy(group => group.Key)
                .Select(group => new DonationByDayDto(group.Key, group.Sum(donation => donation.Amount)))
                .ToList(),
            DonationsByCampaign = confirmedDonations
                .GroupBy(donation => new { donation.CampaignName, donation.GoalAmount })
                .OrderByDescending(group => group.Sum(donation => donation.Amount))
                .Select(group => new DonationByCampaignDto(group.Key.CampaignName, group.Sum(donation => donation.Amount), group.Key.GoalAmount))
                .ToList(),
            DonationsByProject = confirmedDonations
                .GroupBy(donation => new { donation.ProjectName, donation.ProjectGoalAmount })
                .OrderByDescending(group => group.Sum(donation => donation.Amount))
                .Select(group => new DonationByProjectDto(group.Key.ProjectName, group.Sum(donation => donation.Amount), group.Key.ProjectGoalAmount))
                .ToList(),
            LatestDonations = confirmedDonations
                .OrderByDescending(donation => donation.PaidAt)
                .Take(5)
                .Select(donation => new LatestDonationDto(donation.Id, donation.DonorName, donation.Amount, donation.PaidAt, donation.CampaignName))
                .ToList(),
            PriorityActions =
            [
                new("priority-overdue-tasks", "Tarefas vencidas", $"{overdueTasks} contatos precisam de retorno.", "/tarefas", new Dictionary<string, string> { ["due"] = "Overdue" }, overdueTasks > 0 ? "red" : "blue"),
                new("priority-overdue-donations", "Cobranças vencidas", $"{pendingOrOverdue} contribuições exigem acompanhamento.", "/contribuicoes", new Dictionary<string, string> { ["status"] = "Overdue" }, pendingOrOverdue > 0 ? "yellow" : "blue"),
                new("priority-risk-donors", "Doadores em risco", $"{atRiskDonors} doadores precisam de acao de retencao.", "/doadores", new Dictionary<string, string> { ["segment"] = "AtRisk" }, atRiskDonors > 0 ? "yellow" : "blue"),
                new("priority-leads", "Leads sem conversao", $"{leadsWithoutDonation} cadastros ainda sem primeira contribuição.", "/doadores", new Dictionary<string, string> { ["segment"] = "LeadsWithoutDonation" }, "blue"),
                new("priority-documents", "Cadastros incompletos", $"{missingDocuments} doadores estáo sem documento cadastrado.", "/doadores", new Dictionary<string, string> { ["documentStatus"] = "Missing" }, missingDocuments > 0 ? "yellow" : "blue"),
                new("priority-alerts", "Alertas operacionais", $"{openOperationalAlerts} alertas abertos para a equipe.", "/alertas", new Dictionary<string, string> { ["status"] = "Open" }, highOperationalAlerts > 0 ? "red" : openOperationalAlerts > 0 ? "yellow" : "blue"),
            ],
            Funnel =
            [
                new("Cadastros na base", totalDonors, "100%"),
                new("Com contato registrado", contactedDonors, Percent(contactedDonors, totalDonors)),
                new("Com contribuição", donorsWithDonation, Percent(donorsWithDonation, totalDonors)),
                new("Recorrentes", recurringDonors, Percent(recurringDonors, totalDonors)),
            ],
            TeamPerformance = await TeamPerformance(cancellationToken),
            OperationalHealth =
            [
                new("Cadastros incompletos", missingDocuments.ToString(CultureInfo.InvariantCulture), missingDocuments > 0 ? "yellow" : "green"),
                new("Sem contato registrado", Math.Max(totalDonors - contactedDonors, 0).ToString(CultureInfo.InvariantCulture), totalDonors - contactedDonors > 0 ? "red" : "green"),
                new("Consentimento OK", $"{consentOk}%", consentOk >= 80 ? "green" : "yellow"),
                new("Tarefas abertas", openTasks.ToString(CultureInfo.InvariantCulture), openTasks > 0 ? "blue" : "green"),
                new("Alertas abertos", openOperationalAlerts.ToString(CultureInfo.InvariantCulture), highOperationalAlerts > 0 ? "red" : openOperationalAlerts > 0 ? "yellow" : "green"),
            ],
            OpenOperationalAlertsCount = openOperationalAlerts,
            HighOperationalAlertsCount = highOperationalAlerts,
        };
    }

    private async Task<IReadOnlyCollection<TeamPerformanceDto>> TeamPerformance(CancellationToken cancellationToken)
    {
        var tasks = await _context.RelationshipTasks
            .AsNoTracking()
            .Where(task => task.AssignedUserId != null)
            .GroupBy(task => task.AssignedUserId!)
            .Select(group => new
            {
                UserName = group.Key,
                CompletedTasks = group.Count(task => task.Status == RelationshipTaskStatus.Completed),
            })
            .ToListAsync(cancellationToken);

        return tasks
            .OrderByDescending(item => item.CompletedTasks)
            .Take(5)
            .Select(item => new TeamPerformanceDto(item.UserName, item.CompletedTasks, 0, 0))
            .ToList();
    }

    private static string FormatCurrency(decimal amount) =>
        string.Create(CultureInfo.GetCultureInfo("pt-BR"), $"{amount:C}");

    private static string Percent(int value, int total) =>
        total <= 0 ? "0%" : $"{Math.Round(value * 100m / total)}%";

    private static string PeriodLabel(DateTimeOffset start, DateTimeOffset end, DateTimeOffset today) =>
        start.Date == new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero).Date
            ? "Mes atual"
            : $"{start:dd/MM/yyyy} a {end:dd/MM/yyyy}";
}
