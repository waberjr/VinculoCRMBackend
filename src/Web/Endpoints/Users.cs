using System.Security.Claims;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VinculoBackend.Web.Endpoints;

public sealed class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapGet(Me, "me").RequireAuthorization();
        groupBuilder.MapGet(Attendants, "attendants").RequireAuthorization();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    public sealed record LoginRequest(string Email, string Password);

    public sealed record CurrentOrganizationResponse(Guid Id, string Name, decimal? DefaultMonthlyGoal);

    public sealed record CurrentUserResponse(
        string Id,
        string DisplayName,
        string Email,
        string Role,
        CurrentOrganizationResponse Organization);

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

    [EndpointSummary("Current user")]
    [EndpointDescription("Returns the authenticated user and organization context.")]
    public static async Task<Results<Ok<CurrentUserResponse>, UnauthorizedHttpResult, NotFound>> Me(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        if (user.OrganizationId is null)
        {
            return TypedResults.NotFound();
        }

        var organization = await context.Organizations
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(entity => entity.Id == user.OrganizationId.Value)
            .Select(entity => new CurrentOrganizationResponse(
                entity.Id,
                entity.Name,
                entity.DefaultMonthlyGoal))
            .FirstOrDefaultAsync();

        if (organization is null)
        {
            return TypedResults.NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var response = new CurrentUserResponse(
            user.Id,
            string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
            user.Email ?? string.Empty,
            roles.FirstOrDefault() ?? "Agent",
            organization);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Attendants")]
    [EndpointDescription("Returns active users from the current organization for assignment fields.")]
    public static async Task<Results<Ok<IReadOnlyCollection<AttendantResponse>>, UnauthorizedHttpResult>> Attendants(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser?.OrganizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        var users = await userManager.Users
            .AsNoTracking()
            .Where(user => user.OrganizationId == currentUser.OrganizationId && user.IsActive)
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .Select(user => new AttendantResponse(
                user.Id,
                string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
                user.Email ?? string.Empty))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyCollection<AttendantResponse>>(users);
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user.")]
    public static async Task<Ok> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.Ok();
    }
}
