using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class DonationPlan : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public string? AssignedUserId { get; set; }
    public decimal ExpectedAmount { get; private set; }
    public PaymentMethod PreferredPaymentMethod { get; set; }
    public int BillingDay { get; private set; }
    public DateTimeOffset StartDateUtc { get; set; }
    public DonationPlanStatus Status { get; set; }
    public DateTimeOffset? PausedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }

    public void SetExpectedAmount(decimal expectedAmount)
    {
        if (expectedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedAmount), "Donation plan amount must be greater than zero.");
        }

        ExpectedAmount = expectedAmount;
    }

    public void SetBillingDay(int billingDay)
    {
        if (billingDay is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(billingDay), "Billing day must be between 1 and 28.");
        }

        BillingDay = billingDay;
    }
}
