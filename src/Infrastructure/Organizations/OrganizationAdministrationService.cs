using System.Security.Cryptography;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Identity;
using ApplicationNotFoundException = VinculoBackend.Application.Common.Exceptions.NotFoundException;

namespace VinculoBackend.Infrastructure.Organizations;

public sealed class OrganizationAdministrationService : IOrganizationAdministrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrganizationDefaultsService _organizationDefaultsService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizationAdministrationService(
        ApplicationDbContext context,
        IOrganizationDefaultsService organizationDefaultsService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _organizationDefaultsService = organizationDefaultsService;
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<OrganizationResponse>> ListAsync(string userId, CancellationToken cancellationToken)
    {
        var currentUser = await RequiredUserAsync(userId);
        if (await _userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return await _context.Organizations
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
                .ToListAsync(cancellationToken);
        }

        return await QueryUserOrganizations(userId).ToListAsync(cancellationToken);
    }

    public async Task<OrganizationResponse> CreateAsync(string userId, CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await RequiredSystemAdministratorAsync(userId);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ThrowValidation(nameof(request.Name), "O nome da organização é obrigatório.");
        }

        var organization = new Organization
        {
            Name = request.Name.Trim(),
            LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim(),
            Document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim(),
            DefaultMonthlyGoal = request.DefaultMonthlyGoal,
            IsActive = true,
        };

        _context.Organizations.Add(organization);
        await _organizationDefaultsService.EnsureDefaultsAsync(organization.Id, cancellationToken);
        _context.OrganizationMembers.Add(new OrganizationMember
        {
            OrganizationId = organization.Id,
            UserId = currentUser.Id,
            Role = Roles.Administrator,
            IsActive = true,
            JoinedAtUtc = DateTimeOffset.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.LegalName,
            organization.Document,
            organization.DefaultMonthlyGoal,
            Roles.Administrator);
    }

    public async Task UpdateAsync(string userId, Guid organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        await RequiredSystemAdministratorAsync(userId);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ThrowValidation(nameof(request.Name), "O nome da organização é obrigatório.");
        }

        var organization = await _context.Organizations.FirstOrDefaultAsync(item => item.Id == organizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(Organization), organizationId.ToString());

        organization.Name = request.Name.Trim();
        organization.LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim();
        organization.Document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim();
        organization.DefaultMonthlyGoal = request.DefaultMonthlyGoal;
        organization.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string userId, Guid organizationId, CancellationToken cancellationToken)
    {
        await RequiredSystemAdministratorAsync(userId);
        var organization = await _context.Organizations.FirstOrDefaultAsync(item => item.Id == organizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(Organization), organizationId.ToString());

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationTeamResponse> GetTeamAsync(string userId, Guid organizationId, CancellationToken cancellationToken)
    {
        if (!await CanManageUsersAsync(userId, organizationId))
        {
            throw new ForbiddenAccessException();
        }

        var members = await _context.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.OrganizationId == organizationId)
            .Join(
                _userManager.Users.AsNoTracking(),
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    Member = member,
                    User = user,
                    DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? user.UserName ?? "Usuário" : user.DisplayName,
                })
            .OrderBy(item => item.DisplayName)
            .Select(item => new OrganizationMemberResponse(
                item.Member.Id,
                item.User.Id,
                item.DisplayName,
                item.User.Email ?? string.Empty,
                item.Member.Role,
                item.Member.IsActive))
            .ToListAsync(cancellationToken);

        var invitations = await _context.OrganizationInvitations
            .AsNoTracking()
            .Where(invitation => invitation.OrganizationId == organizationId && !invitation.IsRevoked && invitation.AcceptedAtUtc == null)
            .OrderByDescending(invitation => invitation.Created)
            .Select(invitation => new OrganizationInvitationResponse(
                invitation.Id,
                invitation.Email,
                invitation.Role,
                invitation.ExpiresAtUtc,
                invitation.AcceptedAtUtc,
                invitation.IsRevoked))
            .ToListAsync(cancellationToken);

        return new OrganizationTeamResponse(members, invitations);
    }

    public async Task UpdateMemberAsync(string userId, Guid organizationId, Guid memberId, UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        if (!await CanManageUsersAsync(userId, organizationId))
        {
            throw new ForbiddenAccessException();
        }

        if (!IsValidOrganizationRole(request.Role))
        {
            ThrowValidation(nameof(request.Role), "Informe um papel válido.");
        }

        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(item => item.Id == memberId && item.OrganizationId == organizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(OrganizationMember), memberId.ToString());

        member.Role = request.Role;
        member.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMemberAsync(string userId, Guid organizationId, Guid memberId, CancellationToken cancellationToken)
    {
        if (!await CanManageUsersAsync(userId, organizationId))
        {
            throw new ForbiddenAccessException();
        }

        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(item => item.Id == memberId && item.OrganizationId == organizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(OrganizationMember), memberId.ToString());

        _context.OrganizationMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<InviteUserResponse> InviteAsync(string userId, Guid? activeOrganizationId, InviteUserRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await RequiredUserAsync(userId);
        var isSystemAdministrator = await _userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator);
        var organizationId = isSystemAdministrator && request.OrganizationId is not null
            ? request.OrganizationId.Value
            : activeOrganizationId ?? throw new UnauthorizedAccessException();

        if (!isSystemAdministrator && !await CanManageUsersAsync(userId, organizationId))
        {
            throw new ForbiddenAccessException();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !IsValidOrganizationRole(request.Role))
        {
            ThrowValidation(nameof(request.Email), "Informe um e-mail e um papel válidos.");
        }

        var organizationExists = await _context.Organizations
            .AsNoTracking()
            .AnyAsync(organization => organization.Id == organizationId && organization.IsActive, cancellationToken);

        if (!organizationExists)
        {
            ThrowValidation(nameof(request.OrganizationId), "Informe uma organização ativa válida.");
        }

        var invitedUser = await _userManager.FindByEmailAsync(email);
        if (invitedUser is not null)
        {
            var alreadyMember = await _context.OrganizationMembers
                .AsNoTracking()
                .AnyAsync(member => member.OrganizationId == organizationId && member.UserId == invitedUser.Id && member.IsActive, cancellationToken);

            if (alreadyMember)
            {
                ThrowValidation(nameof(request.Email), "O usuário já pertence a está organização.");
            }
        }

        var invitation = new OrganizationInvitation
        {
            OrganizationId = organizationId,
            Email = email,
            Role = request.Role,
            Token = CreateInvitationToken(),
            InvitedByUserId = currentUser.Id,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7),
        };

        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);

        return new InviteUserResponse(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.Token,
            invitation.ExpiresAtUtc,
            invitedUser is not null);
    }

    public async Task RevokeInvitationAsync(string userId, Guid organizationId, Guid invitationId, CancellationToken cancellationToken)
    {
        if (!await CanManageUsersAsync(userId, organizationId))
        {
            throw new ForbiddenAccessException();
        }

        var invitation = await _context.OrganizationInvitations
            .FirstOrDefaultAsync(item => item.Id == invitationId && item.OrganizationId == organizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(OrganizationInvitation), invitationId.ToString());

        _context.OrganizationInvitations.Remove(invitation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationResponse> AcceptInvitationAsync(string? userId, string token, AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var invitation = await _context.OrganizationInvitations
            .FirstOrDefaultAsync(item => item.Token == token, cancellationToken)
            ?? throw new ApplicationNotFoundException(nameof(OrganizationInvitation), token);

        if (invitation.IsRevoked || invitation.AcceptedAtUtc is not null || invitation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            ThrowValidation(nameof(token), "O convite não está ativo.");
        }

        var currentUser = string.IsNullOrWhiteSpace(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (currentUser is not null && !string.Equals(currentUser.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            ThrowValidation(nameof(invitation.Email), "O e-mail do convite não corresponde ao usuário autenticado.");
        }

        currentUser ??= await _userManager.FindByEmailAsync(invitation.Email);
        if (currentUser is null)
        {
            var password = request.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                ThrowValidation(nameof(request.Password), "A senha é obrigatória para criar o usuário convidado.");
            }

            currentUser = new ApplicationUser
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? invitation.Email : request.DisplayName.Trim(),
                EmailConfirmed = true,
                IsActive = true,
            };

            var createResult = await _userManager.CreateAsync(currentUser, password!);
            if (!createResult.Succeeded)
            {
                ThrowValidation(nameof(request.Password), string.Join(" ", createResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await _context.OrganizationMembers.AnyAsync(member => member.OrganizationId == invitation.OrganizationId && member.UserId == currentUser.Id, cancellationToken))
        {
            _context.OrganizationMembers.Add(new OrganizationMember
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
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Organizations
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
    }

    private IQueryable<OrganizationResponse> QueryUserOrganizations(string userId)
    {
        return _context.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId && member.IsActive)
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
            .Select(item => new OrganizationResponse(
                item.Organization.Id,
                item.Organization.Name,
                item.Organization.LegalName,
                item.Organization.Document,
                item.Organization.DefaultMonthlyGoal,
                item.Role));
    }

    private async Task<ApplicationUser> RequiredUserAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId) ?? throw new UnauthorizedAccessException();
    }

    private async Task<ApplicationUser> RequiredSystemAdministratorAsync(string userId)
    {
        var user = await RequiredUserAsync(userId);
        if (!await _userManager.IsInRoleAsync(user, Roles.SystemAdministrator))
        {
            throw new ForbiddenAccessException();
        }

        return user;
    }

    private async Task<bool> CanManageUsersAsync(string userId, Guid organizationId)
    {
        var currentUser = await RequiredUserAsync(userId);
        if (await _userManager.IsInRoleAsync(currentUser, Roles.SystemAdministrator))
        {
            return true;
        }

        var role = await _context.OrganizationMembers
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

    private static string CreateInvitationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }

    private static void ThrowValidation(string propertyName, string message)
    {
        throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }
}
