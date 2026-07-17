namespace VinculoBackend.Domain.Entities;

public class OperationalAlertAuditEntry : OrganizationEntity
{
    public Guid OperationalAlertId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
