using System.Security.Claims;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Web.Services;

public class OrganizationContext : IOrganizationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid OrganizationId
    {
        get
        {
            var value = FindOrganizationClaim();
            return Guid.TryParse(value, out var organizationId) ? organizationId : Guid.Empty;
        }
    }

    public bool HasOrganization => OrganizationId != Guid.Empty;

    private string? FindOrganizationClaim()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue("organization_id")
            ?? user?.FindFirstValue("OrganizationId")
            ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Org-Id"].FirstOrDefault();
    }
}
