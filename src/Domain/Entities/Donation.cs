using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Enums;

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
    public DonationType Type { get; set; }
    public DonationStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
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
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor da contribuição deve ser maior que zero.");
        }

        Amount = amount;
    }

    public void Confirm(DateTimeOffset paidAtUtc, string? reference)
    {
        if (Status is not (DonationStatus.Pending or DonationStatus.Overdue))
        {
            throw new InvalidOperationException("Apenas contribuições pendentes ou vencidas podem ser confirmadas.");
        }

        Status = DonationStatus.Confirmed;
        PaidAtUtc = paidAtUtc;
        Reference = reference?.Trim();
    }

    public void Cancel(string reason, DateTimeOffset cancelledAtUtc)
    {
        if (Status is not (DonationStatus.Pending or DonationStatus.Overdue))
        {
            throw new InvalidOperationException("Apenas contribuições pendentes ou vencidas podem ser canceladas.");
        }

        Status = DonationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = reason.Trim();
    }

    public void Refund(string reason, DateTimeOffset refundedAtUtc)
    {
        if (Status != DonationStatus.Confirmed)
        {
            throw new InvalidOperationException("Apenas contribuições confirmadas podem ser estornadas.");
        }

        Status = DonationStatus.Refunded;
        RefundedAtUtc = refundedAtUtc;
        RefundReason = reason.Trim();
    }
}
