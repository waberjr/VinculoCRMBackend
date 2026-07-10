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

        var existingReceipt = await _context.Receipts
            .AsNoTracking()
            .Where(receipt => receipt.DonationId == request.DonationId)
            .Select(receipt => new { receipt.Id, receipt.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingReceipt is not null)
        {
            if (existingReceipt.Status == ReceiptStatus.Cancelled)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(IssueReceiptCommand.DonationId), "Esta contribuicao possui recibo cancelado e deve ser tratada pela tela de recibos."),
                ]);
            }

            return existingReceipt.Id;
        }

        var donation = await _context.Donations
            .FirstOrDefaultAsync(entity => entity.Id == request.DonationId, cancellationToken);

        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.DonationId.ToString());
        }

        var organization = await _context.Organizations.FirstAsync(entity => entity.Id == organizationId, cancellationToken);
        var receiptNumber = $"{organization.ReceiptNumberPrefix}-{organization.ReceiptNumberNextSequence:00000}";
        organization.ReceiptNumberNextSequence += 1;

        var receipt = Receipt.Issue(organizationId, donation, receiptNumber, _user.Id, DateTimeOffset.UtcNow);

        _context.Receipts.Add(receipt);
        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Recibo emitido",
            Description = $"Recibo {receipt.Number} emitido para contribuicao de {receipt.Amount:C}.",
            OccurredAtUtc = receipt.IssuedAtUtc!.Value,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Receipt),
            RelatedEntityId = receipt.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }
}
