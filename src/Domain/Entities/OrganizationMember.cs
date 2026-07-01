namespace VinculoBackend.Domain.Entities;

public class OrganizationMember : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "Agent";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
