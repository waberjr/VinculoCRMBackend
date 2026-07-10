using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

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

        donation.Cancel(request.Reason, DateTimeOffset.UtcNow);

        var context = await _context.Donations
            .AsNoTracking()
            .Where(entity => entity.Id == donation.Id)
            .Select(entity => new
            {
                CampaignName = entity.Campaign == null ? null : entity.Campaign.Name,
                ProjectName = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == entity.Id)
                    .Select(projectLink => projectLink.Project.Name)
                    .FirstOrDefault(),
            })
            .FirstAsync(cancellationToken);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = donation.OrganizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = "Contribuicao cancelada",
            Description = DonationTimelineDescription(donation.CancellationReason, context.CampaignName, context.ProjectName),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string DonationTimelineDescription(string? reason, string? campaignName, string? projectName)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(reason))
        {
            parts.Add(reason);
        }

        if (!string.IsNullOrWhiteSpace(campaignName))
        {
            parts.Add($"Campanha: {campaignName}.");
        }

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            parts.Add($"Projeto/destinacao: {projectName}.");
        }

        return string.Join(" ", parts);
    }
}
