namespace VinculoBackend.Application.ImpactProjects.Models;

public sealed class ProjectListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal GoalAmount { get; init; }
    public decimal RaisedAmount { get; init; }
    public int DonorsCount { get; init; }
    public string? ImpactMetric { get; init; }
    public IReadOnlyCollection<ProjectCampaignDto> Campaigns { get; init; } = [];
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed class ProjectCampaignDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class ProjectAccountabilityDto
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal GoalAmount { get; init; }
    public decimal RaisedAmount { get; init; }
    public int DonorsCount { get; init; }
    public int DonationsCount { get; init; }
    public Guid? FilterCampaignId { get; init; }
    public string? FilterCampaignName { get; init; }
    public DateTimeOffset? FilterStartDateUtc { get; init; }
    public DateTimeOffset? FilterEndDateUtc { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public IReadOnlyCollection<ProjectCampaignDto> AvailableCampaigns { get; init; } = [];
    public IReadOnlyCollection<ProjectCampaignAccountabilityDto> Campaigns { get; init; } = [];
    public IReadOnlyCollection<ProjectPeriodAccountabilityDto> Periods { get; init; } = [];
    public IReadOnlyCollection<ProjectDonationAccountabilityDto> Donations { get; init; } = [];
    public IReadOnlyCollection<ProjectImpactUpdateAccountabilityDto> ImpactUpdates { get; init; } = [];
}

public sealed class ProjectCampaignAccountabilityDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal RaisedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
    public decimal AverageDonationAmount { get; init; }
    public decimal SharePercentage { get; init; }
}

public sealed class ProjectDonationAccountabilityDto
{
    public Guid Id { get; init; }
    public Guid DonorId { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public Guid? CampaignId { get; init; }
    public string? CampaignName { get; init; }
    public Guid? ReceiptId { get; init; }
    public string? ReceiptNumber { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset? PaidAtUtc { get; init; }
    public string? Reference { get; init; }
}

public sealed class ProjectPeriodAccountabilityDto
{
    public string Period { get; init; } = string.Empty;
    public decimal RaisedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
}

public sealed class ProjectImpactUpdateAccountabilityDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset PublishedAtUtc { get; init; }
}
