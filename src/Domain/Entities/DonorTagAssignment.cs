namespace VinculoBackend.Domain.Entities;

public class DonorTagAssignment : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid DonorTagId { get; set; }
    public DonorTag DonorTag { get; set; } = null!;
}
