using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Communications.Models;

namespace VinculoBackend.Application.Communications.Queries.GetCommunicationCampaigns;

public sealed record GetCommunicationCampaignsQuery : IRequest<IReadOnlyCollection<CommunicationCampaignDto>>;

public sealed class GetCommunicationCampaignsQueryHandler : IRequestHandler<GetCommunicationCampaignsQuery, IReadOnlyCollection<CommunicationCampaignDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CommunicationCampaignDto>> Handle(GetCommunicationCampaignsQuery request, CancellationToken cancellationToken)
    {
        return await _context.CommunicationCampaigns
            .AsNoTracking()
            .OrderByDescending(campaign => campaign.PlannedAtUtc)
            .Select(campaign => new CommunicationCampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Channel = campaign.Channel.ToString(),
                Status = campaign.Status.ToString(),
                Audience = campaign.Audience,
                TemplateId = campaign.TemplateId,
                TemplateName = campaign.Template == null ? null : campaign.Template.Name,
                ScheduledAtUtc = campaign.ScheduledAtUtc,
                PlannedAtUtc = campaign.PlannedAtUtc,
                RecipientsCount = campaign.RecipientsCount,
                BlockedByConsentCount = campaign.BlockedByConsentCount,
            })
            .ToListAsync(cancellationToken);
    }
}
