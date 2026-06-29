namespace VinculoBackend.Domain.Entities;

public class DonorEmail : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid TypeOptionId { get; set; }
    public ConfigurableOption TypeOption { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
