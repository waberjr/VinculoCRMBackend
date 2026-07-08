using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Receipts.Queries.ValidateReceipt;

public sealed record ValidateReceiptQuery(Guid Id, string? Code) : IRequest<ReceiptValidationDto?>;

public sealed class ValidateReceiptQueryHandler : IRequestHandler<ValidateReceiptQuery, ReceiptValidationDto?>
{
    private readonly IApplicationDbContext _context;

    public ValidateReceiptQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReceiptValidationDto?> Handle(ValidateReceiptQuery request, CancellationToken cancellationToken)
    {
        var verificationCode = request.Id.ToString("N")[..12].ToUpperInvariant();
        if (!string.Equals(request.Code?.Trim(), verificationCode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return await _context.Receipts
            .AsNoTracking()
            .Where(receipt =>
                receipt.Id == request.Id &&
                (receipt.Status == ReceiptStatus.Issued || receipt.Status == ReceiptStatus.Reissued) &&
                receipt.IssuedAtUtc != null &&
                receipt.Donation.PaidAtUtc != null)
            .Select(receipt => new ReceiptValidationDto
            {
                Id = receipt.Id,
                Number = receipt.Number,
                VerificationCode = verificationCode,
                OrganizationName = receipt.Organization.Name,
                DonorName = receipt.Donor.FullName,
                Amount = receipt.Amount,
                PaidAtUtc = receipt.Donation.PaidAtUtc!.Value,
                IssuedAtUtc = receipt.IssuedAtUtc!.Value,
                Status = receipt.Status.ToString(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
