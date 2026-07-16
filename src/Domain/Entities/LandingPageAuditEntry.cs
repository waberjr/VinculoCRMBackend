namespace VinculoBackend.Domain.Entities;

public class LandingPageAuditEntry : OrganizationEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
