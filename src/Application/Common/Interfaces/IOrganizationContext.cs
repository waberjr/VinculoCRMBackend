namespace VinculoBackend.Application.Common.Interfaces;

public interface IOrganizationContext
{
    Guid OrganizationId { get; }
    bool HasOrganization { get; }
}
