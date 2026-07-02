using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
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

        var donation = await _context.Donations.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.Id.ToString());
        }

        if (donation.Status is not (DonationStatus.Pending or DonationStatus.Overdue))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(CancelDonationCommand.Id), "Apenas contribuicoes pendentes ou vencidas podem ser canceladas."),
            ]);
        }

        donation.Cancel(request.Reason, DateTimeOffset.UtcNow);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = donation.OrganizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Contribuicao cancelada",
            Description = donation.CancellationReason,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
