using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Communications.Services;

public interface ICommunicationCampaignRecipientPlanner
{
    Task PlanRecipientsAsync(CommunicationCampaign campaign, IReadOnlyCollection<Guid> donorIds, CancellationToken cancellationToken);
}

public sealed class CommunicationCampaignRecipientPlanner : ICommunicationCampaignRecipientPlanner
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CommunicationCampaignRecipientPlanner(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task PlanRecipientsAsync(CommunicationCampaign campaign, IReadOnlyCollection<Guid> donorIds, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var selectedDonorIds = donorIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        var donorsQuery = _context.Donors.AsNoTracking();
        if (selectedDonorIds.Length > 0)
        {
            donorsQuery = donorsQuery.Where(donor => selectedDonorIds.Contains(donor.Id));
        }

        var donors = await donorsQuery
            .Select(donor => new
            {
                donor.Id,
                donor.AllowsCommunication,
                donor.DoNotContact,
            })
            .ToListAsync(cancellationToken);

        foreach (var donor in donors)
        {
            var canContact = donor.AllowsCommunication && !donor.DoNotContact;
            DonorTimelineEntry? timeline = null;
            if (canContact)
            {
                timeline = new DonorTimelineEntry
                {
                    OrganizationId = organizationId,
                    DonorId = donor.Id,
                    Type = TimelineEntryType.Contact,
                    Title = $"Comunicacao planejada: {campaign.Name}",
                    Description = $"Canal: {campaign.Channel}. Publico: {campaign.Audience}. Sem envio real.",
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    CreatedByUserId = _user.Id,
                    RelatedEntityType = nameof(CommunicationCampaign),
                    RelatedEntityId = campaign.Id,
                };
                _context.DonorTimelineEntries.Add(timeline);
            }

            var recipient = campaign.AddRecipient(organizationId, donor.Id, canContact, timeline?.Id);
            _context.CommunicationCampaignRecipients.Add(recipient);
        }
    }
}
