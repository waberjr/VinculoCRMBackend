namespace VinculoBackend.Application.Dashboard.Models;

public sealed record DashboardMetricDto(string Label, string Value, string Helper, string Trend);

public sealed record DonationByDayDto(string Day, decimal Amount);

public sealed record DonationByCampaignDto(string CampaignName, decimal Amount, decimal GoalAmount);

public sealed record DonationByProjectDto(string ProjectName, decimal Amount, decimal GoalAmount);

public sealed record LatestDonationDto(Guid Id, string DonorName, decimal Amount, DateTimeOffset PaidAt, string CampaignName);

public sealed record PriorityActionDto(
    string Id,
    string Label,
    string Description,
    string Route,
    IReadOnlyDictionary<string, string> QueryParams,
    string Tone);

public sealed record FunnelStageDto(string Label, int Count, string ConversionRate);

public sealed record TeamPerformanceDto(string UserName, int CompletedTasks, int DonationsConfirmed, decimal AmountConfirmed);

public sealed record OperationalHealthItemDto(string Label, string Value, string Tone);

public sealed class DashboardOverviewDto
{
    public IReadOnlyCollection<DashboardMetricDto> Metrics { get; init; } = [];
    public IReadOnlyCollection<DonationByDayDto> DonationsByDay { get; init; } = [];
    public IReadOnlyCollection<DonationByCampaignDto> DonationsByCampaign { get; init; } = [];
    public IReadOnlyCollection<DonationByProjectDto> DonationsByProject { get; init; } = [];
    public IReadOnlyCollection<LatestDonationDto> LatestDonations { get; init; } = [];
    public IReadOnlyCollection<PriorityActionDto> PriorityActions { get; init; } = [];
    public IReadOnlyCollection<FunnelStageDto> Funnel { get; init; } = [];
    public IReadOnlyCollection<TeamPerformanceDto> TeamPerformance { get; init; } = [];
    public IReadOnlyCollection<OperationalHealthItemDto> OperationalHealth { get; init; } = [];
}
