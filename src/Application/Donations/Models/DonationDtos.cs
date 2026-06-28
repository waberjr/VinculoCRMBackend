using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Donations.Models;

public sealed class DonationListItemDto
{
    public Guid Id { get; init; }
    public Guid DonorId { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public Guid? CampaignId { get; init; }
    public string? CampaignName { get; init; }
    public decimal Amount { get; init; }
    public OptionDto Type { get; init; } = new();
    public OptionDto Status { get; init; } = new();
    public OptionDto PaymentMethod { get; init; } = new();
    public DateTimeOffset? ExpectedAtUtc { get; init; }
    public DateTimeOffset? PaidAtUtc { get; init; }
    public string? Reference { get; init; }
}
