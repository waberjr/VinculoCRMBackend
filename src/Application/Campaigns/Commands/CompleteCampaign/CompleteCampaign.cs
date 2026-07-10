using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Commands.CompleteCampaign;

public record CompleteCampaignCommand(Guid Id) : IRequest;

public sealed class CompleteCampaignCommandHandler : IRequestHandler<CompleteCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CompleteCampaignCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(CompleteCampaignCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var campaign = await _context.Campaigns.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (campaign is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.Id.ToString());
        }

        campaign.Complete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
