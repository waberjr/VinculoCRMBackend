using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

namespace VinculoBackend.Application.Donations.Commands.CancelDonation;

public record CancelDonationCommand(Guid Id, string Reason) : IRequest;

public sealed class CancelDonationCommandHandler : IRequestHandler<CancelDonationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CancelDonationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(CancelDonationCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donation = await _context.Donations
            .Include(entity => entity.StatusOption)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.Id.ToString());
        }

        if (donation.StatusOption.Code is not ("pending" or "overdue"))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(CancelDonationCommand.Id), "Apenas contribuicoes pendentes ou vencidas podem ser canceladas."),
            ]);
        }

        donation.Cancel(
            await OptionLookup.RequiredIdAsync(_context, "DonationStatus", "Cancelled", cancellationToken),
            request.Reason,
            donation.StatusOption.Code,
            DateTimeOffset.UtcNow);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = donation.OrganizationId,
            DonorId = donation.DonorId,
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", "Donation", cancellationToken),
            Title = "Contribuicao cancelada",
            Description = donation.CancellationReason,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
