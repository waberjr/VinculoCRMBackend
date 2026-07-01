using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.ActivateCampaign;

public record ActivateCampaignCommand(Guid Id) : IRequest;

public sealed class ActivateCampaignCommandHandler : IRequestHandler<ActivateCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public ActivateCampaignCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var campaign = await _context.Campaigns.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (campaign is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.Id.ToString());
        }

        campaign.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "CampaignStatus", "Active", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
