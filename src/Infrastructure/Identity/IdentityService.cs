using System.Security.Claims;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Users.Models;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace VinculoBackend.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
    private readonly TimeProvider _timeProvider;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _context = context;
        _signInManager = signInManager;
        _bearerTokenOptions = bearerTokenOptions;
        _timeProvider = timeProvider;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> PasswordSignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive)
        {
            return false;
        }

        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName ?? email,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        return result.Succeeded;
    }

    public async Task<ClaimsPrincipal?> RefreshSignInPrincipalAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var refreshTicket = options.RefreshTokenProtector.Unprotect(refreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            _timeProvider.GetUtcNow() >= expiresUtc)
        {
            return null;
        }

        var user = await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        return await _signInManager.CreateUserPrincipalAsync(user);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(string userId, Guid? requestedOrganizationId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var isSystemAdministrator = await _userManager.IsInRoleAsync(user, Roles.SystemAdministrator);
        var organizations = isSystemAdministrator
            ? await _context.Organizations
                .AsNoTracking()
                .Where(organization => organization.IsActive)
                .OrderBy(organization => organization.Name)
                .Select(organization => new CurrentOrganizationDto(
                    organization.Id,
                    organization.Name,
                    organization.DefaultMonthlyGoal,
                    Roles.Administrator))
                .ToListAsync(cancellationToken)
            : await _context.OrganizationMembers
                .AsNoTracking()
                .Where(member => member.UserId == user.Id && member.IsActive)
                .Join(
                    _context.Organizations.AsNoTracking().Where(organization => organization.IsActive),
                    member => member.OrganizationId,
                    organization => organization.Id,
                    (member, organization) => new
                    {
                        member.Role,
                        Organization = organization,
                    })
                .OrderBy(item => item.Organization.Name)
                .Select(item => new CurrentOrganizationDto(
                    item.Organization.Id,
                    item.Organization.Name,
                    item.Organization.DefaultMonthlyGoal,
                    item.Role))
                .ToListAsync(cancellationToken);

        if (organizations.Count == 0 && user.OrganizationId is not null)
        {
            var legacyOrganization = await _context.Organizations
                .AsNoTracking()
                .Where(entity => entity.Id == user.OrganizationId.Value && entity.IsActive)
                .Select(entity => new CurrentOrganizationDto(
                    entity.Id,
                    entity.Name,
                    entity.DefaultMonthlyGoal,
                    Roles.Agent))
                .FirstOrDefaultAsync(cancellationToken);

            if (legacyOrganization is not null)
            {
                organizations.Add(legacyOrganization);
            }
        }

        if (organizations.Count == 0)
        {
            return null;
        }

        var organization = organizations.FirstOrDefault(item => item.Id == requestedOrganizationId)
            ?? organizations.FirstOrDefault(item => item.Id == user.OrganizationId)
            ?? organizations[0];

        return new CurrentUserDto(
            user.Id,
            string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
            user.Email ?? string.Empty,
            organization.Role,
            isSystemAdministrator,
            organization,
            organizations);
    }

    public async Task<IReadOnlyCollection<AttendantDto>> GetAttendantsAsync(string userId, Guid organizationId, CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser is null || !currentUser.IsActive)
        {
            return [];
        }

        var isSystemAdministrator = await _userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator);
        var canAccessOrganization = isSystemAdministrator ||
            await _context.OrganizationMembers
                .AsNoTracking()
                .AnyAsync(member => member.UserId == currentUser.Id && member.OrganizationId == organizationId && member.IsActive, cancellationToken);

        if (!canAccessOrganization)
        {
            return [];
        }

        return await _userManager.Users
            .AsNoTracking()
            .Join(
                _context.OrganizationMembers.AsNoTracking().Where(member => member.OrganizationId == organizationId && member.IsActive),
                user => user.Id,
                member => member.UserId,
                (user, member) => user)
            .Where(user => user.IsActive)
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .Select(user => new AttendantDto(
                user.Id,
                string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuario" : user.DisplayName,
                user.Email ?? string.Empty))
            .ToListAsync(cancellationToken);
    }
}
