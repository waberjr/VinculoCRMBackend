using System.Security.Claims;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace VinculoBackend.Web.Endpoints;

public sealed class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(Refresh, "refresh");
        groupBuilder.MapGet(Me, "me").RequireAuthorization();
        groupBuilder.MapGet(Attendants, "attendants").RequireAuthorization();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    public sealed record LoginRequest(string Email, string Password);

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record CurrentOrganizationResponse(Guid Id, string Name, decimal? DefaultMonthlyGoal, string Role);

    public sealed record CurrentUserResponse(
        string Id,
        string DisplayName,
        string Email,
        string Role,
        bool IsPlatformAdministrator,
        CurrentOrganizationResponse Organization,
        IReadOnlyCollection<CurrentOrganizationResponse> Organizations);

    public sealed record AttendantResponse(string Id, string DisplayName, string Email);

    [EndpointSummary("Log in")]
    [EndpointDescription("Authenticates a user and returns a bearer token response.")]
    public static async Task<Results<EmptyHttpResult, ProblemHttpResult>> Login(
        SignInManager<ApplicationUser> signInManager,
        [FromBody] LoginRequest request)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        return result.Succeeded
            ? TypedResults.Empty
            : TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials.");
    }

    [EndpointSummary("Refresh token")]
    [EndpointDescription("Returns a new bearer token response using a valid refresh token.")]
    public static async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> Refresh(
        SignInManager<ApplicationUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider,
        [FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var options = bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var refreshTicket = options.RefreshTokenProtector.Unprotect(request.RefreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc)
        {
            return TypedResults.Unauthorized();
        }

        var user = await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal);
        if (user is null || !user.IsActive)
        {
            return TypedResults.Unauthorized();
        }

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    [EndpointSummary("Current user")]
    [EndpointDescription("Returns the authenticated user and organization context.")]
    public static async Task<Results<Ok<CurrentUserResponse>, UnauthorizedHttpResult, NotFound>> Me(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var memberships = await context.OrganizationMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(member => member.UserId == user.Id && member.IsActive)
            .Join(
                context.Organizations.AsNoTracking().IgnoreQueryFilters().Where(organization => organization.IsActive),
                member => member.OrganizationId,
                organization => organization.Id,
                (member, organization) => new
                {
                    member.Role,
                    Organization = organization,
                })
            .OrderBy(item => item.Organization.Name)
            .Select(item => new CurrentOrganizationResponse(
                    item.Organization.Id,
                    item.Organization.Name,
                    item.Organization.DefaultMonthlyGoal,
                    item.Role))
            .ToListAsync();

        if (memberships.Count == 0 && user.OrganizationId is not null)
        {
            var legacyOrganization = await context.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(entity => entity.Id == user.OrganizationId.Value && entity.IsActive)
                .Select(entity => new CurrentOrganizationResponse(
                    entity.Id,
                    entity.Name,
                    entity.DefaultMonthlyGoal,
                    Roles.Agent))
                .FirstOrDefaultAsync();

            if (legacyOrganization is not null)
            {
                memberships.Add(legacyOrganization);
            }
        }

        if (memberships.Count == 0)
        {
            return TypedResults.NotFound();
        }

        var requestedOrganizationId = TryGetRequestedOrganizationId(principal, httpContext);
        var organization = memberships.FirstOrDefault(item => item.Id == requestedOrganizationId)
            ?? memberships.FirstOrDefault(item => item.Id == user.OrganizationId)
            ?? memberships[0];

        var roles = await userManager.GetRolesAsync(user);
        var response = new CurrentUserResponse(
            user.Id,
            string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
            user.Email ?? string.Empty,
            organization.Role,
            roles.Contains(Roles.Administrator),
            organization,
            memberships);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Attendants")]
    [EndpointDescription("Returns active users from the current organization for assignment fields.")]
    public static async Task<Results<Ok<IReadOnlyCollection<AttendantResponse>>, UnauthorizedHttpResult>> Attendants(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        var organizationId = TryGetRequestedOrganizationId(principal, httpContext) ?? currentUser?.OrganizationId;
        if (currentUser is null || organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        var canAccessOrganization = await userManager.IsInRoleAsync(currentUser, Roles.Administrator) ||
            await context.OrganizationMembers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AnyAsync(member => member.UserId == currentUser.Id && member.OrganizationId == organizationId && member.IsActive);

        if (!canAccessOrganization)
        {
            return TypedResults.Unauthorized();
        }

        var users = await userManager.Users
            .AsNoTracking()
            .Join(
                context.OrganizationMembers.AsNoTracking().IgnoreQueryFilters().Where(member => member.OrganizationId == organizationId && member.IsActive),
                user => user.Id,
                member => member.UserId,
                (user, member) => user)
            .Where(user => user.IsActive)
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .Select(user => new AttendantResponse(
                user.Id,
                string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
                user.Email ?? string.Empty))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyCollection<AttendantResponse>>(users);
    }

    private static Guid? TryGetRequestedOrganizationId(ClaimsPrincipal principal, HttpContext httpContext)
    {
        var value = principal.FindFirstValue("organization_id")
            ?? principal.FindFirstValue("OrganizationId")
            ?? httpContext.Request.Headers["X-Org-Id"].FirstOrDefault();
        return Guid.TryParse(value, out var organizationId) ? organizationId : null;
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user.")]
    public static async Task<Ok> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.Ok();
    }
}
