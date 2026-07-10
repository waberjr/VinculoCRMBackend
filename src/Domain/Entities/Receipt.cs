using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

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

    public static Receipt Issue(Guid organizationId, Donation donation, string number, string? issuedByUserId, DateTimeOffset issuedAtUtc)
    {
        if (donation.Status != DonationStatus.Confirmed || donation.PaidAtUtc is null)
        {
            throw new InvalidOperationDomainException("Recibos so podem ser emitidos para contribuicoes confirmadas.");
        }

        return new Receipt
        {
            OrganizationId = organizationId,
            DonationId = donation.Id,
            DonorId = donation.DonorId,
            Number = number,
            Amount = donation.Amount,
            Status = ReceiptStatus.Issued,
            IssuedAtUtc = issuedAtUtc,
            FileUrl = null,
            IssuedByUserId = issuedByUserId,
        };
    }

    public void Cancel(string reason)
    {
        if (Status == ReceiptStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("O recibo ja esta cancelado.");
        }

        Status = ReceiptStatus.Cancelled;
        CancelReason = reason.Trim();
    }

    public void Reissue(string? issuedByUserId, DateTimeOffset issuedAtUtc)
    {
        if (Status == ReceiptStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Recibos cancelados nao podem ser reemitidos.");
        }

        Status = ReceiptStatus.Reissued;
        IssuedAtUtc = issuedAtUtc;
        FileUrl = null;
        IssuedByUserId = issuedByUserId;
    }
}
