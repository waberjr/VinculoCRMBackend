using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Receipts.Commands.ReissueReceipt;

public record ReissueReceiptCommand(Guid Id, string Reason) : IRequest;

public sealed class ReissueReceiptCommandHandler : IRequestHandler<ReissueReceiptCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public ReissueReceiptCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(ReissueReceiptCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var receipt = await _context.Receipts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (receipt is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Receipt), request.Id.ToString());
        }

        receipt.Reissue(_user.Id, DateTimeOffset.UtcNow);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = receipt.OrganizationId,
            DonorId = receipt.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Recibo reemitido",
            Description = $"Recibo {receipt.Number} reemitido. Motivo: {request.Reason.Trim()}",
            OccurredAtUtc = receipt.IssuedAtUtc!.Value,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Receipt),
            RelatedEntityId = receipt.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
