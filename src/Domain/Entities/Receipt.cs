using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class Receipt : OrganizationEntity
{
    public Guid DonationId { get; set; }
    public Donation Donation { get; set; } = null!;
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public string Number { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ReceiptStatus Status { get; set; }
    public DateTimeOffset? IssuedAtUtc { get; set; }
    public string? FileUrl { get; set; }
    public string? CancelReason { get; set; }
    public string? IssuedByUserId { get; set; }
}
