namespace VinculoBackend.Domain.Entities;

public abstract class OrganizationEntity : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}
