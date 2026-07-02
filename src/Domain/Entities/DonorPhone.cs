using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class DonorPhone : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public PhoneType Type { get; set; }
    public string Number { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
