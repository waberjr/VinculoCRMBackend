namespace VinculoBackend.Domain.Entities;

public class OrganizationInvitation : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Agent";
    public string Token { get; set; } = string.Empty;
    public string InvitedByUserId { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? AcceptedAtUtc { get; set; }
    public string? AcceptedByUserId { get; set; }
    public bool IsRevoked { get; set; }
}
