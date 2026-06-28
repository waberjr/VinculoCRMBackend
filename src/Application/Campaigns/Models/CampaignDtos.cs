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
