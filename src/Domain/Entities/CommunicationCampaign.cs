using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class CommunicationCampaign : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public CommunicationChannel Channel { get; set; } = CommunicationChannel.Email;
    public CommunicationCampaignStatus Status { get; set; } = CommunicationCampaignStatus.Draft;
    public string Audience { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public CommunicationTemplate? Template { get; set; }
    public DateTimeOffset? ScheduledAtUtc { get; set; }
    public DateTimeOffset PlannedAtUtc { get; set; }
    public int RecipientsCount { get; set; }
    public int BlockedByConsentCount { get; set; }
    public string? CreatedByUserId { get; set; }
    public ICollection<CommunicationCampaignRecipient> Recipients { get; } = new List<CommunicationCampaignRecipient>();

    public static CommunicationCampaign Create(
        Guid organizationId,
        string name,
        CommunicationChannel channel,
        string audience,
        Guid? templateId,
        DateTimeOffset? scheduledAtUtc,
        string? createdByUserId)
    {
        var campaign = new CommunicationCampaign
        {
            OrganizationId = organizationId,
            PlannedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = createdByUserId,
        };
        campaign.UpdatePlan(name, channel, audience, templateId, scheduledAtUtc);
        return campaign;
    }

    public void UpdatePlan(
        string name,
        CommunicationChannel channel,
        string audience,
        Guid? templateId,
        DateTimeOffset? scheduledAtUtc)
    {
        EnsureEditable();

        Name = name.Trim();
        Channel = channel;
        Audience = audience.Trim();
        TemplateId = templateId;
        ScheduledAtUtc = scheduledAtUtc;
        Status = scheduledAtUtc is null ? CommunicationCampaignStatus.Draft : CommunicationCampaignStatus.Scheduled;
    }

    public CommunicationCampaignRecipient AddRecipient(Guid organizationId, Guid donorId, bool canContact, Guid? timelineEntryId)
    {
        var recipient = CommunicationCampaignRecipient.Create(organizationId, Id, donorId, canContact, timelineEntryId);
        Recipients.Add(recipient);

        if (canContact)
        {
            RecipientsCount++;
        }
        else
        {
            BlockedByConsentCount++;
        }

        return recipient;
    }

    public void ResetRecipientMetrics()
    {
        RecipientsCount = 0;
        BlockedByConsentCount = 0;
    }

    public void Cancel(string? reason)
    {
        if (Status == CommunicationCampaignStatus.Cancelled)
        {
            return;
        }

        Status = CommunicationCampaignStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Audience = $"{Audience} | Cancelada: {reason.Trim()}";
        }
    }

    private void EnsureEditable()
    {
        if (Status == CommunicationCampaignStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Campanhas canceladas nao podem ser editadas.");
        }
    }
}
