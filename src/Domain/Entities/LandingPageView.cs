namespace VinculoBackend.Domain.Entities;

public class LandingPageView : OrganizationEntity
{
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string FingerprintHash { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public DateTimeOffset WindowStartedAtUtc { get; set; }
    public DateTimeOffset ViewedAtUtc { get; set; }
}
