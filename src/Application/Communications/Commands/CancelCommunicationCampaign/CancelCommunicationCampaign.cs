using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Communications.Commands.CancelCommunicationCampaign;

public sealed record CancelCommunicationCampaignCommand(Guid Id, string Reason) : IRequest;

public sealed class CancelCommunicationCampaignCommandHandler : IRequestHandler<CancelCommunicationCampaignCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelCommunicationCampaignCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelCommunicationCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.CommunicationCampaigns.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (campaign is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(CommunicationCampaign), request.Id.ToString());
        }

        campaign.Cancel(request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
