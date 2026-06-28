using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Common.Models;

public static class RequiredOrganization
{
    public static Guid From(IOrganizationContext organizationContext)
    {
        if (!organizationContext.HasOrganization)
        {
            throw new ForbiddenAccessException();
        }

        return organizationContext.OrganizationId;
    }
}
