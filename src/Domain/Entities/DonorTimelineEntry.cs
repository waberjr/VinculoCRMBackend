using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class DonorTimelineEntry : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public TimelineEntryType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
