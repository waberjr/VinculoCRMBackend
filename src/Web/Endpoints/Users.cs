using VinculoBackend.Application.Users.Commands.LoginUser;
using VinculoBackend.Application.Users.Commands.RefreshUserToken;
using VinculoBackend.Application.Users.Models;
using VinculoBackend.Application.Users.Queries.GetAttendants;
using VinculoBackend.Application.Users.Queries.GetCurrentUser;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    [EndpointSummary("Log in")]
    [EndpointDescription("Authenticates a user and returns a bearer token response.")]
    public static async Task<Results<EmptyHttpResult, ProblemHttpResult>> Login(
        ISender sender,
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var succeeded = await sender.Send(new LoginUserCommand(request.Email, request.Password), cancellationToken);
        return succeeded
            ? TypedResults.Empty
            : TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials.");
    }

    [EndpointSummary("Refresh token")]
    [EndpointDescription("Returns a new bearer token response using a valid refresh token.")]
    public static async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> Refresh(
        ISender sender,
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var principal = await sender.Send(new RefreshUserTokenCommand(request.RefreshToken), cancellationToken);
        return principal is null
            ? TypedResults.Unauthorized()
            : TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    [EndpointSummary("Current user")]
    [EndpointDescription("Returns the authenticated user and organization context.")]
    public static async Task<Results<Ok<CurrentUserDto>, UnauthorizedHttpResult, NotFound>> Me(ISender sender)
    {
        var user = await sender.Send(new GetCurrentUserQuery());
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    [EndpointSummary("Attendants")]
    [EndpointDescription("Returns active users from the current organization for assignment fields.")]
    public static async Task<Ok<IReadOnlyCollection<AttendantDto>>> Attendants(ISender sender)
    {
        var users = await sender.Send(new GetAttendantsQuery());
        return TypedResults.Ok(users);
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user.")]
    public static async Task<Ok> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.Ok();
    }
}
