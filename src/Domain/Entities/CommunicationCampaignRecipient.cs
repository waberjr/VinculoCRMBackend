using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class CommunicationCampaignRecipient : OrganizationEntity
{
    public Guid CommunicationCampaignId { get; set; }
    public CommunicationCampaign CommunicationCampaign { get; set; } = null!;
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public CommunicationRecipientStatus Status { get; set; } = CommunicationRecipientStatus.Planned;
    public string? BlockReason { get; set; }
    public Guid? TimelineEntryId { get; set; }
    public DonorTimelineEntry? TimelineEntry { get; set; }

    public static CommunicationCampaignRecipient Create(
        Guid organizationId,
        Guid communicationCampaignId,
        Guid donorId,
        bool canContact,
        Guid? timelineEntryId)
    {
        return new CommunicationCampaignRecipient
        {
            OrganizationId = organizationId,
            CommunicationCampaignId = communicationCampaignId,
            DonorId = donorId,
            Status = canContact ? CommunicationRecipientStatus.Planned : CommunicationRecipientStatus.Blocked,
            BlockReason = canContact ? null : "Contato bloqueado por consentimento ou DoNotContact.",
            TimelineEntryId = timelineEntryId,
        };
    }

    public void SoftDelete(DateTimeOffset deletedAtUtc)
    {
        IsDeleted = true;
        Deleted = deletedAtUtc;
    }
}
