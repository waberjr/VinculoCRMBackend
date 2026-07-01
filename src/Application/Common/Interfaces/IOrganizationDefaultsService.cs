namespace VinculoBackend.Application.Common.Interfaces;

public interface IOrganizationDefaultsService
{
    Task EnsureDefaultsAsync(Guid organizationId, CancellationToken cancellationToken);
}
