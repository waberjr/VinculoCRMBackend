using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

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

        donation.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonationStatus", "Cancelled", cancellationToken);
        donation.CancelledAtUtc = DateTimeOffset.UtcNow;
        donation.CancellationReason = request.Reason.Trim();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
