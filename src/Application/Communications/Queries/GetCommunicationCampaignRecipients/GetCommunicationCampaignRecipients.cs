using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Communications.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Communications.Queries.GetCommunicationCampaignRecipients;

public sealed record GetCommunicationCampaignRecipientsQuery(Guid CampaignId) : IRequest<IReadOnlyCollection<CommunicationRecipientDto>?>;

public sealed class GetCommunicationCampaignRecipientsQueryHandler : IRequestHandler<GetCommunicationCampaignRecipientsQuery, IReadOnlyCollection<CommunicationRecipientDto>?>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationCampaignRecipientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CommunicationRecipientDto>?> Handle(GetCommunicationCampaignRecipientsQuery request, CancellationToken cancellationToken)
    {
        var campaignExists = await _context.CommunicationCampaigns
            .AsNoTracking()
            .AnyAsync(campaign => campaign.Id == request.CampaignId, cancellationToken);

        if (!campaignExists)
        {
            return null;
        }

        return await _context.CommunicationCampaignRecipients
            .AsNoTracking()
            .Where(recipient => recipient.CommunicationCampaignId == request.CampaignId)
            .OrderBy(recipient => recipient.Status)
            .ThenBy(recipient => recipient.Donor.FullName)
            .Select(recipient => new CommunicationRecipientDto
            {
                Id = recipient.Id,
                DonorId = recipient.DonorId,
                DonorName = recipient.Donor.FullName,
                DonorEmail = recipient.Donor.Email,
                DonorPhone = recipient.Donor.Phone,
                AllowsCommunication = recipient.Donor.AllowsCommunication,
                DoNotContact = recipient.Donor.DoNotContact,
                Status = recipient.Status.ToString(),
                BlockReason = recipient.BlockReason,
                TimelineEntryId = recipient.TimelineEntryId,
            })
            .ToListAsync(cancellationToken);
    }
}
