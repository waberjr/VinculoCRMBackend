using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Receipts.Commands.CancelReceipt;

public record CancelReceiptCommand(Guid Id, string Reason) : IRequest;

public sealed class CancelReceiptCommandHandler : IRequestHandler<CancelReceiptCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CancelReceiptCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(CancelReceiptCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var receipt = await _context.Receipts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (receipt is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Receipt), request.Id.ToString());
        }

        receipt.Cancel(request.Reason);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = receipt.OrganizationId,
            DonorId = receipt.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Recibo cancelado",
            Description = $"Recibo {receipt.Number} cancelado. Motivo: {receipt.CancelReason}",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Receipt),
            RelatedEntityId = receipt.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
