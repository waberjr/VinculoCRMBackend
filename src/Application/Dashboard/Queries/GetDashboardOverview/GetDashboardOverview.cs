using System.Globalization;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Dashboard.Models;

namespace VinculoBackend.Application.Dashboard.Queries.GetDashboardOverview;

public record GetDashboardOverviewQuery : IRequest<DashboardOverviewDto>
{
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
}

public sealed class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDashboardOverviewQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var today = DateTimeOffset.UtcNow;
        var start = request.StartDateUtc ?? new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = request.EndDateUtc ?? start.AddMonths(1).AddTicks(-1);

        var monthlyGoal = await _context.Organizations
            .AsNoTracking()
            .Where(organization => organization.Id == organizationId)
            .Select(organization => organization.DefaultMonthlyGoal ?? 0)
            .FirstOrDefaultAsync(cancellationToken);

        var confirmedDonations = await _context.Donations
            .AsNoTracking()
            .Where(donation =>
                donation.PaidAtUtc != null &&
                donation.PaidAtUtc >= start &&
                donation.PaidAtUtc <= end)
            .Select(donation => new
            {
                donation.Id,
                donation.Amount,
                PaidAt = donation.PaidAtUtc!.Value,
                DonorName = donation.Donor.FullName,
                CampaignName = donation.Campaign == null ? "Sem campanha" : donation.Campaign.Name,
                GoalAmount = donation.Campaign == null ? monthlyGoal : donation.Campaign.GoalAmount ?? 0,
            })
            .ToListAsync(cancellationToken);

        var totalDonors = await _context.Donors.AsNoTracking().CountAsync(cancellationToken);
        var activeDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.StatusOption.Code == "active", cancellationToken);
        var pendingOrOverdue = await _context.Donations.AsNoTracking().CountAsync(donation =>
            donation.StatusOption.Code == "pending" || donation.StatusOption.Code == "overdue", cancellationToken);
        var dueToday = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.DueAtUtc != null &&
            task.DueAtUtc.Value.Date == today.Date &&
            task.StatusOption.Code != "completed" &&
            task.StatusOption.Code != "cancelled", cancellationToken);
        var overdueTasks = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.DueAtUtc != null &&
            task.DueAtUtc.Value.Date < today.Date &&
            task.StatusOption.Code != "completed" &&
            task.StatusOption.Code != "cancelled", cancellationToken);
        var atRiskDonors = await _context.Donors.AsNoTracking().CountAsync(donor => donor.StatusOption.Code == "at-risk", cancellationToken);
        var leadsWithoutDonation = await _context.Donors.AsNoTracking().CountAsync(donor =>
            donor.StatusOption.Code == "lead" && !_context.Donations.Any(donation => donation.DonorId == donor.Id), cancellationToken);
        var missingDocuments = await _context.Donors.AsNoTracking().CountAsync(donor => donor.Document == null || donor.Document == string.Empty, cancellationToken);

        var confirmedAmount = confirmedDonations.Sum(donation => donation.Amount);
        var averageDonation = confirmedDonations.Count == 0 ? 0 : confirmedAmount / confirmedDonations.Count;

        var contactedDonors = await _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry => entry.TypeOption.Code == "contact")
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
            .Where(plan => plan.StatusOption.Code == "active")
            .Select(plan => plan.DonorId)
            .Distinct()
            .CountAsync(cancellationToken);
        var consentOk = totalDonors == 0 ? 0 : (int)Math.Round(
            await _context.Donors.AsNoTracking().CountAsync(donor => donor.AllowsCommunication && !donor.DoNotContact, cancellationToken) * 100m / totalDonors);
        var openTasks = await _context.RelationshipTasks.AsNoTracking().CountAsync(task =>
            task.StatusOption.Code == "open" || task.StatusOption.Code == "in-progress", cancellationToken);

        return new DashboardOverviewDto
        {
            Metrics =
            [
                new("Arrecadacao confirmada", FormatCurrency(confirmedAmount), PeriodLabel(start, end), monthlyGoal <= 0 ? "Meta nao configurada" : $"{Math.Round(confirmedAmount * 100 / monthlyGoal)}% da meta"),
                new("Doadores ativos", activeDonors.ToString(CultureInfo.InvariantCulture), "Status Active", $"{totalDonors} cadastrados"),
                new("Pendencias", pendingOrOverdue.ToString(CultureInfo.InvariantCulture), "Cobrancas pendentes ou vencidas", $"{dueToday} tarefas hoje"),
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
            LatestDonations = confirmedDonations
                .OrderByDescending(donation => donation.PaidAt)
                .Take(5)
                .Select(donation => new LatestDonationDto(donation.Id, donation.DonorName, donation.Amount, donation.PaidAt, donation.CampaignName))
                .ToList(),
            PriorityActions =
            [
                new("priority-overdue-tasks", "Tarefas vencidas", $"{overdueTasks} contatos precisam de retorno.", "/tarefas", new Dictionary<string, string> { ["due"] = "Overdue" }, overdueTasks > 0 ? "red" : "blue"),
                new("priority-overdue-donations", "Cobrancas vencidas", $"{pendingOrOverdue} contribuicoes exigem acompanhamento.", "/contribuicoes", new Dictionary<string, string> { ["status"] = "Overdue" }, pendingOrOverdue > 0 ? "yellow" : "blue"),
                new("priority-risk-donors", "Doadores em risco", $"{atRiskDonors} doadores precisam de acao de retencao.", "/doadores", new Dictionary<string, string> { ["segment"] = "AtRisk" }, atRiskDonors > 0 ? "yellow" : "blue"),
                new("priority-leads", "Leads sem conversao", $"{leadsWithoutDonation} cadastros ainda sem primeira contribuicao.", "/doadores", new Dictionary<string, string> { ["segment"] = "LeadsWithoutDonation" }, "blue"),
                new("priority-documents", "Cadastros incompletos", $"{missingDocuments} doadores estao sem documento cadastrado.", "/doadores", new Dictionary<string, string>(), missingDocuments > 0 ? "yellow" : "blue"),
            ],
            Funnel =
            [
                new("Cadastros na base", totalDonors, "100%"),
                new("Com contato registrado", contactedDonors, Percent(contactedDonors, totalDonors)),
                new("Com contribuicao", donorsWithDonation, Percent(donorsWithDonation, totalDonors)),
                new("Recorrentes", recurringDonors, Percent(recurringDonors, totalDonors)),
            ],
            TeamPerformance = await TeamPerformance(cancellationToken),
            OperationalHealth =
            [
                new("Cadastros incompletos", missingDocuments.ToString(CultureInfo.InvariantCulture), missingDocuments > 0 ? "yellow" : "green"),
                new("Sem contato registrado", Math.Max(totalDonors - contactedDonors, 0).ToString(CultureInfo.InvariantCulture), totalDonors - contactedDonors > 0 ? "red" : "green"),
                new("Consentimento OK", $"{consentOk}%", consentOk >= 80 ? "green" : "yellow"),
                new("Tarefas abertas", openTasks.ToString(CultureInfo.InvariantCulture), openTasks > 0 ? "blue" : "green"),
            ],
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
                CompletedTasks = group.Count(task => task.StatusOption.Code == "completed"),
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

    private static string PeriodLabel(DateTimeOffset start, DateTimeOffset end) =>
        start.Date == new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero).Date
            ? "Mes atual"
            : $"{start:dd/MM/yyyy} a {end:dd/MM/yyyy}";
}
