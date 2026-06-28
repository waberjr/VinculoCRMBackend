using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donations.Commands.ConfirmDonation;

public record ConfirmDonationCommand(Guid Id, Guid ConfirmedStatusOptionId, DateTimeOffset PaidAtUtc) : IRequest;

public sealed class ConfirmDonationCommandHandler : IRequestHandler<ConfirmDonationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public ConfirmDonationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(ConfirmDonationCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donation = await _context.Donations.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.Id.ToString());
        }

        donation.StatusOptionId = request.ConfirmedStatusOptionId;
        donation.PaidAtUtc = request.PaidAtUtc;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
