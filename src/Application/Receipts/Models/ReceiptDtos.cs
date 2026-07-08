using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Receipts.Models;

public sealed class ReceiptListItemDto
{
    public Guid Id { get; init; }
    public Guid DonationId { get; init; }
    public string Number { get; init; } = string.Empty;
    public string DonorName { get; init; } = string.Empty;
    public string? CampaignName { get; init; }
    public string? ProjectName { get; init; }
    public string DonationReference { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public ReceiptStatus Status { get; init; }
    public DateTimeOffset? IssuedAtUtc { get; init; }
    public string? FileUrl { get; init; }
    public string? CancelReason { get; init; }
}

public sealed class ReceiptPrintDto
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public string? OrganizationDocument { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public string? DonorDocument { get; init; }
    public string? CampaignName { get; init; }
    public string? ProjectName { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset PaidAtUtc { get; init; }
    public DateTimeOffset IssuedAtUtc { get; init; }
    public string DonationReference { get; init; } = string.Empty;
}
