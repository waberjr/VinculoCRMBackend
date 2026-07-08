namespace VinculoBackend.Domain.Entities;

public class ImpactUpdate : OrganizationEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset PublishedAtUtc { get; set; }
    public string? CreatedByUserId { get; set; }
}
