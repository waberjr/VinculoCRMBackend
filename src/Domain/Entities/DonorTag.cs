namespace VinculoBackend.Domain.Entities;

using VinculoBackend.Domain.Exceptions;

public class DonorTag : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public static DonorTag Create(Guid organizationId, string name, string? description = null)
    {
        var tag = new DonorTag
        {
            OrganizationId = organizationId,
            IsActive = true,
        };
        tag.Update(name, description, true);

        return tag;
    }

    public void Update(string name, string? description, bool isActive)
    {
        SetName(name);
        Description = TrimToNull(description);
        IsActive = isActive;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Informe o nome da tag.");
        }

        Name = name.Trim();
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
