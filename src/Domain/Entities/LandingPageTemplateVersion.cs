namespace VinculoBackend.Domain.Entities;

public class LandingPageTemplateVersion : OrganizationEntity
{
    public Guid TemplateId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? HeroImageUrl { get; set; }
    public decimal? GoalAmount { get; set; }
    public string? CustomFieldsJson { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
