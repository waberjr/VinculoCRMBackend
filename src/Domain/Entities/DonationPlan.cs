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
            throw new ArgumentOutOfRangeException(nameof(expectedAmount), "O valor do plano recorrente deve ser maior que zero.");
        }

        ExpectedAmount = expectedAmount;
    }

    public void SetBillingDay(int billingDay)
    {
        if (billingDay is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(billingDay), "O dia de cobrança deve estar entre 1 e 31.");
        }

        BillingDay = billingDay;
    }
}
