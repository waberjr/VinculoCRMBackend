using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class DonorEmail : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public EmailType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public static DonorEmail Create(Guid organizationId, Guid donorId, EmailType type, string address, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new DomainValidationException("Informe o e-mail.");
        }

        return new DonorEmail
        {
            OrganizationId = organizationId,
            DonorId = donorId,
            Type = type,
            Address = address.Trim(),
            IsPrimary = isPrimary,
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
