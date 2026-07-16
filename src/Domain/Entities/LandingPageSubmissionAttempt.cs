namespace VinculoBackend.Domain.Entities;

public class LandingPageSubmissionAttempt : OrganizationEntity
{
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string FingerprintHash { get; set; } = string.Empty;
    public string? Source { get; set; }
    public bool Blocked { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
}
