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
