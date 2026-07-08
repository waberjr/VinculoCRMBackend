namespace VinculoBackend.Domain.Entities;

public class DocumentAttachmentAuditEntry : OrganizationEntity
{
    public Guid DocumentAttachmentId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
