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
    public Guid TypeOptionId { get; set; }
    public ConfigurableOption TypeOption { get; set; } = null!;
    public Guid PriorityOptionId { get; set; }
    public ConfigurableOption PriorityOption { get; set; } = null!;
    public Guid StatusOptionId { get; set; }
    public ConfigurableOption StatusOption { get; set; } = null!;
    public DateTimeOffset? DueAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public Guid? ContactOutcomeOptionId { get; set; }
    public ConfigurableOption? ContactOutcomeOption { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CompletionNote { get; set; }
}
