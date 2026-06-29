namespace VinculoBackend.Domain.Entities;

public class DonorPhone : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid TypeOptionId { get; set; }
    public ConfigurableOption TypeOption { get; set; } = null!;
    public string Number { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
