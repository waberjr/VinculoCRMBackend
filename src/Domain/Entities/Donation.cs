using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

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

    public static Donation Create(
        Guid organizationId,
        Guid donorId,
        Guid? campaignId,
        Guid? donationPlanId,
        decimal amount,
        DonationType type,
        DonationStatus status,
        PaymentMethod paymentMethod,
        DateTimeOffset? expectedAtUtc,
        DateTimeOffset? paidAtUtc,
        string? reference,
        string? externalPaymentId,
        string? notes,
        string? createdByUserId)
    {
        ValidateInitialDates(status, expectedAtUtc, paidAtUtc);

        var donation = new Donation
        {
            OrganizationId = organizationId,
            DonorId = donorId,
            CampaignId = campaignId,
            DonationPlanId = donationPlanId,
            Type = type,
            Status = status,
            PaymentMethod = paymentMethod,
            ExpectedAtUtc = expectedAtUtc,
            PaidAtUtc = paidAtUtc,
            Reference = TrimToNull(reference),
            ExternalPaymentId = TrimToNull(externalPaymentId),
            Notes = TrimToNull(notes),
            CreatedByUserId = createdByUserId,
        };
        donation.SetAmount(amount);

        return donation;
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("O valor da contribuicao deve ser maior que zero.");
        }

        Amount = amount;
    }

    public void Confirm(DateTimeOffset paidAtUtc, string? reference)
    {
        if (Status is not (DonationStatus.Pending or DonationStatus.Overdue))
        {
            throw new InvalidOperationDomainException("Apenas contribuicoes pendentes ou vencidas podem ser confirmadas.");
        }

        Status = DonationStatus.Confirmed;
        PaidAtUtc = paidAtUtc;
        Reference = reference?.Trim();
    }

    public void Cancel(string reason, DateTimeOffset cancelledAtUtc)
    {
        if (Status is not (DonationStatus.Pending or DonationStatus.Overdue))
        {
            throw new InvalidOperationDomainException("Apenas contribuicoes pendentes ou vencidas podem ser canceladas.");
        }

        Status = DonationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = RequiredReason(reason, "Informe o motivo do cancelamento.");
    }

    public void Refund(string reason, DateTimeOffset refundedAtUtc)
    {
        if (Status != DonationStatus.Confirmed)
        {
            throw new InvalidOperationDomainException("Apenas contribuicoes confirmadas podem ser estornadas.");
        }

        Status = DonationStatus.Refunded;
        RefundedAtUtc = refundedAtUtc;
        RefundReason = RequiredReason(reason, "Informe o motivo do estorno.");
    }

    private static void ValidateInitialDates(DonationStatus status, DateTimeOffset? expectedAtUtc, DateTimeOffset? paidAtUtc)
    {
        if (status == DonationStatus.Confirmed && paidAtUtc is null)
        {
            throw new DomainValidationException("A data de pagamento e obrigatoria para contribuicoes confirmadas.");
        }

        if (status is DonationStatus.Pending or DonationStatus.Overdue && expectedAtUtc is null)
        {
            throw new DomainValidationException("A data esperada e obrigatoria para contribuicoes pendentes ou vencidas.");
        }
    }

    private static string RequiredReason(string reason, string message)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException(message);
        }

        return reason.Trim();
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
