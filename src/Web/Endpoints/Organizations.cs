using System.Security.Claims;
using System.Security.Cryptography;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VinculoBackend.Web.Endpoints;

public sealed class Organizations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(List, "").RequireAuthorization();
        groupBuilder.MapPost(Create, "").RequireAuthorization();
        groupBuilder.MapPut(Update, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(Delete, "{id}").RequireAuthorization();
        groupBuilder.MapGet(Members, "current/members").RequireAuthorization();
        groupBuilder.MapPut(UpdateMember, "current/members/{memberId}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteMember, "current/members/{memberId}").RequireAuthorization();
        groupBuilder.MapPost(Invite, "current/invitations").RequireAuthorization();
        groupBuilder.MapDelete(RevokeInvitation, "current/invitations/{invitationId}").RequireAuthorization();
        groupBuilder.MapPost(AcceptInvitation, "invitations/{token}/accept");
    }

    public sealed record OrganizationResponse(Guid Id, string Name, string? LegalName, string? Document, decimal? DefaultMonthlyGoal, string Role);

    public sealed record CreateOrganizationRequest(string Name, string? LegalName, string? Document, decimal? DefaultMonthlyGoal);

    public sealed record UpdateOrganizationRequest(string Name, string? LegalName, string? Document, decimal? DefaultMonthlyGoal, bool IsActive = true);

    public sealed record OrganizationMemberResponse(
        Guid Id,
        string UserId,
        string DisplayName,
        string Email,
        string Role,
        bool IsActive);

    public sealed record OrganizationInvitationResponse(
        Guid Id,
        string Email,
        string Role,
        DateTimeOffset ExpiresAtUtc,
        DateTimeOffset? AcceptedAtUtc,
        bool IsRevoked);

    public sealed record OrganizationTeamResponse(
        IReadOnlyCollection<OrganizationMemberResponse> Members,
        IReadOnlyCollection<OrganizationInvitationResponse> Invitations);

    public sealed record InviteUserRequest(string Email, string Role, Guid? OrganizationId = null);

    public sealed record UpdateMemberRequest(string Role, bool IsActive = true);

    public sealed record InviteUserResponse(Guid Id, string Email, string Role, string Token, DateTimeOffset ExpiresAtUtc, bool UserExists);

    public sealed record AcceptInvitationRequest(string? DisplayName, string? Password);

    [EndpointSummary("Organizations")]
    [EndpointDescription("Returns organizations available to the current user.")]
    public static async Task<Ok<IReadOnlyCollection<OrganizationResponse>>> List(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is null)
        {
            return TypedResults.Ok<IReadOnlyCollection<OrganizationResponse>>([]);
        }

        var isSystemAdministrator = await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator);
        if (isSystemAdministrator)
        {
            var organizations = await context.Organizations
                .AsNoTracking()
                .Where(organization => organization.IsActive)
                .OrderBy(organization => organization.Name)
                .Select(organization => new OrganizationResponse(
                    organization.Id,
                    organization.Name,
                    organization.LegalName,
                    organization.Document,
                    organization.DefaultMonthlyGoal,
                    Roles.Administrator))
                .ToListAsync();

            return TypedResults.Ok<IReadOnlyCollection<OrganizationResponse>>(organizations);
        }

        var memberships = await QueryUserOrganizations(context, currentUser.Id).ToListAsync();
        return TypedResults.Ok<IReadOnlyCollection<OrganizationResponse>>(memberships);
    }

    [EndpointSummary("Create organization")]
    [EndpointDescription("Creates an organization. Restricted to platform administrators.")]
    public static async Task<Results<Created<OrganizationResponse>, ForbidHttpResult, BadRequest<string>>> Create(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IOrganizationDefaultsService organizationDefaultsService,
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is null || !await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return TypedResults.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Organization name is required.");
        }

        var organization = new Organization
        {
            Name = request.Name.Trim(),
            LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim(),
            Document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim(),
            DefaultMonthlyGoal = request.DefaultMonthlyGoal,
            IsActive = true,
        };

        context.Organizations.Add(organization);
        await organizationDefaultsService.EnsureDefaultsAsync(organization.Id, cancellationToken);
        context.OrganizationMembers.Add(new OrganizationMember
        {
            OrganizationId = organization.Id,
            UserId = currentUser.Id,
            Role = Roles.Administrator,
            IsActive = true,
            JoinedAtUtc = DateTimeOffset.UtcNow,
        });

        await context.SaveChangesAsync(cancellationToken);

        var response = new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.LegalName,
            organization.Document,
            organization.DefaultMonthlyGoal,
            Roles.Administrator);

        return TypedResults.Created($"/api/Organizations/{organization.Id}", response);
    }

    [EndpointSummary("Update organization")]
    [EndpointDescription("Updates organization data. Restricted to platform administrators.")]
    public static async Task<Results<NoContent, ForbidHttpResult, NotFound, BadRequest<string>>> Update(
        Guid id,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is null || !await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return TypedResults.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Organization name is required.");
        }

        var organization = await context.Organizations.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (organization is null)
        {
            return TypedResults.NotFound();
        }

        organization.Name = request.Name.Trim();
        organization.LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim();
        organization.Document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim();
        organization.DefaultMonthlyGoal = request.DefaultMonthlyGoal;
        organization.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete organization")]
    [EndpointDescription("Soft deletes an organization. Restricted to platform administrators.")]
    public static async Task<Results<NoContent, ForbidHttpResult, NotFound, BadRequest<string>>> Delete(
        Guid id,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is null || !await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return TypedResults.Forbid();
        }

        var organization = await context.Organizations.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (organization is null)
        {
            return TypedResults.NotFound();
        }

        context.Organizations.Remove(organization);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Organization team")]
    [EndpointDescription("Returns members and pending invitations for the active organization.")]
    public static async Task<Results<Ok<OrganizationTeamResponse>, ForbidHttpResult, UnauthorizedHttpResult>> Members(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        var organizationId = TryGetRequestedOrganizationId(principal, httpContext);
        if (currentUser is null || organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!await CanManageUsersAsync(userManager, context, currentUser, organizationId.Value))
        {
            return TypedResults.Forbid();
        }

        var members = await context.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.OrganizationId == organizationId.Value)
            .Join(
                userManager.Users.AsNoTracking(),
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    Member = member,
                    User = user,
                    DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
                })
            .OrderBy(item => item.DisplayName)
            .Select(item => new OrganizationMemberResponse(
                item.Member.Id,
                item.User.Id,
                item.DisplayName,
                item.User.Email ?? string.Empty,
                item.Member.Role,
                item.Member.IsActive))
            .ToListAsync();

        var invitations = await context.OrganizationInvitations
            .AsNoTracking()
            .Where(invitation => invitation.OrganizationId == organizationId.Value && !invitation.IsRevoked && invitation.AcceptedAtUtc == null)
            .OrderByDescending(invitation => invitation.Created)
            .Select(invitation => new OrganizationInvitationResponse(
                invitation.Id,
                invitation.Email,
                invitation.Role,
                invitation.ExpiresAtUtc,
                invitation.AcceptedAtUtc,
                invitation.IsRevoked))
            .ToListAsync();

        return TypedResults.Ok(new OrganizationTeamResponse(members, invitations));
    }

    [EndpointSummary("Update member")]
    [EndpointDescription("Updates a member role or active status in the active organization.")]
    public static async Task<Results<NoContent, ForbidHttpResult, UnauthorizedHttpResult, NotFound, BadRequest<string>>> UpdateMember(
        Guid memberId,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext,
        [FromBody] UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        var organizationId = TryGetRequestedOrganizationId(principal, httpContext);
        if (currentUser is null || organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!await CanManageUsersAsync(userManager, context, currentUser, organizationId.Value))
        {
            return TypedResults.Forbid();
        }

        if (!IsValidOrganizationRole(request.Role))
        {
            return TypedResults.BadRequest("A valid role is required.");
        }

        var member = await context.OrganizationMembers
            .FirstOrDefaultAsync(item => item.Id == memberId && item.OrganizationId == organizationId.Value, cancellationToken);

        if (member is null)
        {
            return TypedResults.NotFound();
        }

        member.Role = request.Role;
        member.IsActive = request.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete member")]
    [EndpointDescription("Soft deletes a member from the active organization.")]
    public static async Task<Results<NoContent, ForbidHttpResult, UnauthorizedHttpResult, NotFound>> DeleteMember(
        Guid memberId,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        var organizationId = TryGetRequestedOrganizationId(principal, httpContext);
        if (currentUser is null || organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!await CanManageUsersAsync(userManager, context, currentUser, organizationId.Value))
        {
            return TypedResults.Forbid();
        }

        var member = await context.OrganizationMembers
            .FirstOrDefaultAsync(item => item.Id == memberId && item.OrganizationId == organizationId.Value, cancellationToken);

        if (member is null)
        {
            return TypedResults.NotFound();
        }

        context.OrganizationMembers.Remove(member);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Invite user")]
    [EndpointDescription("Creates an invitation to join the active organization.")]
    public static async Task<Results<Created<InviteUserResponse>, ForbidHttpResult, UnauthorizedHttpResult, BadRequest<string>>> Invite(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext,
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is null)
        {
            return TypedResults.Unauthorized();
        }

        var isSystemAdministrator = await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator);
        var organizationId = isSystemAdministrator && request.OrganizationId is not null
            ? request.OrganizationId
            : TryGetRequestedOrganizationId(principal, httpContext);

        if (organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!isSystemAdministrator && !await CanManageUsersAsync(userManager, context, currentUser, organizationId.Value))
        {
            return TypedResults.Forbid();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !IsValidOrganizationRole(request.Role))
        {
            return TypedResults.BadRequest("A valid email and role are required.");
        }

        var organizationExists = await context.Organizations
            .AsNoTracking()
            .AnyAsync(organization => organization.Id == organizationId.Value && organization.IsActive, cancellationToken);

        if (!organizationExists)
        {
            return TypedResults.BadRequest("A valid active organization is required.");
        }

        var invitedUser = await userManager.FindByEmailAsync(email);
        if (invitedUser is not null)
        {
            var alreadyMember = await context.OrganizationMembers
                .AsNoTracking()
                .AnyAsync(member => member.OrganizationId == organizationId.Value && member.UserId == invitedUser.Id && member.IsActive, cancellationToken);

            if (alreadyMember)
            {
                return TypedResults.BadRequest("User already belongs to this organization.");
            }
        }

        var invitation = new OrganizationInvitation
        {
            OrganizationId = organizationId.Value,
            Email = email,
            Role = request.Role,
            Token = CreateInvitationToken(),
            InvitedByUserId = currentUser.Id,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7),
        };

        context.OrganizationInvitations.Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        var response = new InviteUserResponse(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.Token,
            invitation.ExpiresAtUtc,
            invitedUser is not null);

        return TypedResults.Created($"/api/Organizations/invitations/{invitation.Token}", response);
    }

    [EndpointSummary("Revoke invitation")]
    [EndpointDescription("Revokes a pending invitation for the active organization.")]
    public static async Task<Results<NoContent, ForbidHttpResult, UnauthorizedHttpResult, NotFound>> RevokeInvitation(
        Guid invitationId,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(principal);
        var organizationId = TryGetRequestedOrganizationId(principal, httpContext);
        if (currentUser is null || organizationId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!await CanManageUsersAsync(userManager, context, currentUser, organizationId.Value))
        {
            return TypedResults.Forbid();
        }

        var invitation = await context.OrganizationInvitations
            .FirstOrDefaultAsync(item => item.Id == invitationId && item.OrganizationId == organizationId.Value, cancellationToken);

        if (invitation is null)
        {
            return TypedResults.NotFound();
        }

        context.OrganizationInvitations.Remove(invitation);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Accept organization invitation")]
    [EndpointDescription("Accepts an invitation for the authenticated user's email.")]
    public static async Task<Results<Ok<OrganizationResponse>, NotFound, BadRequest<string>>> AcceptInvitation(
        string token,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var invitation = await context.OrganizationInvitations
            .FirstOrDefaultAsync(item => item.Token == token, cancellationToken);

        if (invitation is null)
        {
            return TypedResults.NotFound();
        }

        if (invitation.IsRevoked || invitation.AcceptedAtUtc is not null || invitation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return TypedResults.BadRequest("Invitation is not active.");
        }

        var currentUser = await userManager.GetUserAsync(principal);
        if (currentUser is not null && !string.Equals(currentUser.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest("Invitation email does not match the authenticated user.");
        }

        currentUser ??= await userManager.FindByEmailAsync(invitation.Email);
        if (currentUser is null)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return TypedResults.BadRequest("Password is required to create the invited user.");
            }

            currentUser = new ApplicationUser
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? invitation.Email : request.DisplayName.Trim(),
                EmailConfirmed = true,
                IsActive = true,
            };

            var createResult = await userManager.CreateAsync(currentUser, request.Password);
            if (!createResult.Succeeded)
            {
                return TypedResults.BadRequest(string.Join(" ", createResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await context.OrganizationMembers.AnyAsync(member => member.OrganizationId == invitation.OrganizationId && member.UserId == currentUser.Id, cancellationToken))
        {
            context.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = invitation.OrganizationId,
                UserId = currentUser.Id,
                Role = invitation.Role,
                IsActive = true,
                JoinedAtUtc = DateTimeOffset.UtcNow,
            });
        }

        invitation.AcceptedAtUtc = DateTimeOffset.UtcNow;
        invitation.AcceptedByUserId = currentUser.Id;
        await context.SaveChangesAsync(cancellationToken);

        var organization = await context.Organizations
            .AsNoTracking()
            .Where(item => item.Id == invitation.OrganizationId)
            .Select(item => new OrganizationResponse(
                item.Id,
                item.Name,
                item.LegalName,
                item.Document,
                item.DefaultMonthlyGoal,
                invitation.Role))
            .FirstAsync(cancellationToken);

        return TypedResults.Ok(organization);
    }

    private static IQueryable<OrganizationResponse> QueryUserOrganizations(ApplicationDbContext context, string userId)
    {
        return context.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId && member.IsActive)
            .Join(
                context.Organizations.AsNoTracking().Where(organization => organization.IsActive),
                member => member.OrganizationId,
                organization => organization.Id,
                (member, organization) => new
                {
                    member.Role,
                    Organization = organization,
                })
            .OrderBy(item => item.Organization.Name)
            .Select(item => new OrganizationResponse(
                item.Organization.Id,
                item.Organization.Name,
                item.Organization.LegalName,
                item.Organization.Document,
                item.Organization.DefaultMonthlyGoal,
                item.Role));
    }

    private static async Task<bool> CanManageUsersAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ApplicationUser currentUser,
        Guid organizationId)
    {
        if (await userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return true;
        }

        var role = await context.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.UserId == currentUser.Id && member.OrganizationId == organizationId && member.IsActive)
            .Select(member => member.Role)
            .FirstOrDefaultAsync();

        return role is Roles.Administrator or Roles.Manager;
    }

    private static bool IsValidOrganizationRole(string role)
    {
        return role is Roles.Administrator or Roles.Manager or Roles.Agent;
    }

    private static Guid? TryGetRequestedOrganizationId(ClaimsPrincipal principal, HttpContext httpContext)
    {
        var value = principal.FindFirstValue("organization_id")
            ?? principal.FindFirstValue("OrganizationId")
            ?? httpContext.Request.Headers["X-Org-Id"].FirstOrDefault();

        return Guid.TryParse(value, out var organizationId) ? organizationId : null;
    }

    private static string CreateInvitationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }
}
