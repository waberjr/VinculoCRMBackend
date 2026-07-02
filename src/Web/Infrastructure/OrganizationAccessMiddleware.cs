using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Identity;

namespace VinculoBackend.Web.Infrastructure;

public sealed class OrganizationAccessMiddleware
{
    private readonly RequestDelegate _next;

    public OrganizationAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requestedOrganizationId = FindRequestedOrganizationId(httpContext);

        if (userId is not null && requestedOrganizationId is not null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || !user.IsActive)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var isSystemAdministrator = await userManager.IsInRoleAsync(user, Roles.SystemAdministrator);
            var canAccessOrganization = isSystemAdministrator ||
                user.OrganizationId == requestedOrganizationId.Value ||
                await context.OrganizationMembers
                    .AsNoTracking()
                    .AnyAsync(member =>
                        member.UserId == userId &&
                        member.OrganizationId == requestedOrganizationId.Value &&
                        member.IsActive,
                        httpContext.RequestAborted);

            if (!canAccessOrganization)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await _next(httpContext);
    }

    private static Guid? FindRequestedOrganizationId(HttpContext httpContext)
    {
        var claimValue = httpContext.User.FindFirstValue("organization_id")
            ?? httpContext.User.FindFirstValue("OrganizationId");
        var headerValue = httpContext.Request.Headers["X-Org-Id"].FirstOrDefault();
        var value = claimValue ?? headerValue;

        return Guid.TryParse(value, out var organizationId) ? organizationId : null;
    }
}
