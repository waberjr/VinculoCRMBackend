namespace VinculoBackend.Domain.Entities;

public class DonationProject : OrganizationEntity
{
    public Guid DonationId { get; set; }
    public Donation Donation { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
