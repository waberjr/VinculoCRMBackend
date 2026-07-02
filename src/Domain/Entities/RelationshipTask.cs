using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class RelationshipTask : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public Guid? DonationId { get; set; }
    public Donation? Donation { get; set; }
    public string? AssignedUserId { get; set; }
    public string? CreatedByUserId { get; set; }
    public TaskType Type { get; set; }
    public TaskPriority Priority { get; set; }
    public RelationshipTaskStatus Status { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public ContactOutcome? ContactOutcome { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CompletionNote { get; set; }
}
