namespace VinculoBackend.Application.Campaigns.Models;

public sealed class CampaignListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonorsCount { get; init; }
    public int DonationsCount { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string AssignedUserName { get; init; } = "Sem responsavel";
}

public sealed class CampaignReportDto
{
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public decimal GoalAmount { get; init; }
    public int CampaignsCount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
    public decimal AverageDonationAmount { get; init; }
    public IReadOnlyCollection<CampaignReportItemDto> Campaigns { get; init; } = [];
    public IReadOnlyCollection<CampaignReportPeriodDto> Periods { get; init; } = [];
}

public sealed class CampaignReportItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
    public decimal AverageDonationAmount { get; init; }
    public decimal GoalPercentage { get; init; }
}

public sealed class CampaignReportPeriodDto
{
    public string Period { get; init; } = string.Empty;
    public decimal ConfirmedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
}

public sealed class PublicLandingPageDto
{
    public string Kind { get; init; } = string.Empty;
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonorsCount { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
}

public sealed class LandingPageConfigurationDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal? GoalAmount { get; init; }
    public bool IsActive { get; init; }
}

public sealed class PublicLeadSubmissionDto
{
    public Guid DonorId { get; init; }
    public bool Created { get; init; }
}
