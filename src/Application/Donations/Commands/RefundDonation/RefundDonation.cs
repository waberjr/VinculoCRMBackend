using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donations.Commands.RefundDonation;

public record RefundDonationCommand(Guid Id, string Reason) : IRequest;

public sealed class RefundDonationCommandHandler : IRequestHandler<RefundDonationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public RefundDonationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(RefundDonationCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donation = await _context.Donations.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.Id.ToString());
        }

        donation.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonationStatus", "Refunded", cancellationToken);
        donation.RefundedAtUtc = DateTimeOffset.UtcNow;
        donation.RefundReason = request.Reason.Trim();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
