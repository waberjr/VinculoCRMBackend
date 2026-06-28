namespace VinculoBackend.Domain.Entities;

public class Donation : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public Guid? DonationPlanId { get; set; }
    public DonationPlan? DonationPlan { get; set; }
    public decimal Amount { get; private set; }
    public Guid TypeOptionId { get; set; }
    public ConfigurableOption TypeOption { get; set; } = null!;
    public Guid StatusOptionId { get; set; }
    public ConfigurableOption StatusOption { get; set; } = null!;
    public Guid PaymentMethodOptionId { get; set; }
    public ConfigurableOption PaymentMethodOption { get; set; } = null!;
    public DateTimeOffset? ExpectedAtUtc { get; set; }
    public DateTimeOffset? PaidAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public DateTimeOffset? RefundedAtUtc { get; set; }
    public string? Reference { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public string? RefundReason { get; set; }
    public string? CreatedByUserId { get; set; }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Donation amount must be greater than zero.");
        }

        Amount = amount;
    }
}
