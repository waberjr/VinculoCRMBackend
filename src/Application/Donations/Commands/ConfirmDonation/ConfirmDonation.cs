using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donations.Commands.ConfirmDonation;

public record ConfirmDonationCommand(Guid Id, DateTimeOffset PaidAtUtc, string? Reference) : IRequest;

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

        donation.Confirm(request.PaidAtUtc, request.Reference);

        var projectName = await _context.DonationProjects
            .AsNoTracking()
            .Where(projectLink => projectLink.DonationId == donation.Id)
            .Select(projectLink => projectLink.Project.Name)
            .FirstOrDefaultAsync(cancellationToken);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = donation.OrganizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Contribuicao confirmada",
            Description = projectName is null
                ? $"Pagamento confirmado no valor de {donation.Amount:C}."
                : $"Pagamento confirmado no valor de {donation.Amount:C}. Projeto/destinacao: {projectName}.",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
