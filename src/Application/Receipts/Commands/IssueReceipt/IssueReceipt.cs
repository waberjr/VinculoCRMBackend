using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Receipts.Commands.IssueReceipt;

public record IssueReceiptCommand(Guid DonationId) : IRequest<Guid>;

public sealed class IssueReceiptCommandHandler : IRequestHandler<IssueReceiptCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public IssueReceiptCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(IssueReceiptCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var existingReceiptId = await _context.Receipts
            .AsNoTracking()
            .Where(receipt => receipt.DonationId == request.DonationId && receipt.Status != ReceiptStatus.Cancelled)
            .Select(receipt => (Guid?)receipt.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingReceiptId is not null)
        {
            return existingReceiptId.Value;
        }

        var donation = await _context.Donations
            .FirstOrDefaultAsync(entity => entity.Id == request.DonationId, cancellationToken);

        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.DonationId.ToString());
        }

        if (donation.Status != DonationStatus.Confirmed || donation.PaidAtUtc is null)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(IssueReceiptCommand.DonationId), "Recibos so podem ser emitidos para contribuicoes confirmadas."),
            ]);
        }

        var receipt = new Receipt
        {
            OrganizationId = organizationId,
            DonationId = donation.Id,
            DonorId = donation.DonorId,
            Number = await NextReceiptNumber(organizationId, donation.PaidAtUtc.Value, cancellationToken),
            Amount = donation.Amount,
            Status = ReceiptStatus.Issued,
            IssuedAtUtc = DateTimeOffset.UtcNow,
            FileUrl = null,
            IssuedByUserId = _user.Id,
        };

        _context.Receipts.Add(receipt);
        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Recibo emitido",
            Description = $"Recibo {receipt.Number} emitido para contribuicao de {receipt.Amount:C}.",
            OccurredAtUtc = receipt.IssuedAtUtc.Value,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Receipt),
            RelatedEntityId = receipt.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }

    private async Task<string> NextReceiptNumber(Guid organizationId, DateTimeOffset paidAtUtc, CancellationToken cancellationToken)
    {
        var prefix = $"REC-{paidAtUtc:yyyy}";
        var count = await _context.Receipts
            .AsNoTracking()
            .CountAsync(receipt => receipt.OrganizationId == organizationId && receipt.Number.StartsWith(prefix), cancellationToken);

        return $"{prefix}-{count + 1:00000}";
    }
}
