namespace VinculoBackend.Application.DonationPlans.Models;

public sealed class DonationPlanListItemDto
{
    public Guid Id { get; init; }
    public Guid DonorId { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public Guid? CampaignId { get; init; }
    public decimal ExpectedAmount { get; init; }
    public int BillingDay { get; init; }
    public string PreferredPaymentMethod { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StartDateUtc { get; init; }
    public DateTimeOffset? LastConfirmedAt { get; init; }
    public DateTimeOffset NextExpectedAt { get; init; }
    public string? CampaignName { get; init; }
    public string? Notes { get; init; }
}
