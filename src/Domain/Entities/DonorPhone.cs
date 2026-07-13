using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class DonorPhone : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public PhoneType Type { get; set; }
    public string Number { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public static DonorPhone Create(Guid organizationId, Guid donorId, PhoneType type, string number, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainValidationException("Informe o numero do telefone.");
        }

        return new DonorPhone
        {
            OrganizationId = organizationId,
            DonorId = donorId,
            Type = type,
            Number = number.Trim(),
            IsPrimary = isPrimary,
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
