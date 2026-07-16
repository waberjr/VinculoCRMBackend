namespace VinculoBackend.Domain.Entities;

public class LandingPageBlockRule : OrganizationEntity
{
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string? FingerprintHash { get; set; }
    public string? Source { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
