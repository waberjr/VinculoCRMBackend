using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Common.Interfaces;

public interface IOrganizationAdministrationService
{
    Task<IReadOnlyCollection<OrganizationResponse>> ListAsync(string userId, CancellationToken cancellationToken);

    Task<OrganizationResponse> CreateAsync(string userId, CreateOrganizationRequest request, CancellationToken cancellationToken);

    Task UpdateAsync(string userId, Guid organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(string userId, Guid organizationId, CancellationToken cancellationToken);

    Task<OrganizationTeamResponse> GetTeamAsync(string userId, Guid organizationId, CancellationToken cancellationToken);

    Task UpdateMemberAsync(string userId, Guid organizationId, Guid memberId, UpdateMemberRequest request, CancellationToken cancellationToken);

    Task DeleteMemberAsync(string userId, Guid organizationId, Guid memberId, CancellationToken cancellationToken);

    Task<InviteUserResponse> InviteAsync(string userId, Guid? activeOrganizationId, InviteUserRequest request, CancellationToken cancellationToken);

    Task RevokeInvitationAsync(string userId, Guid organizationId, Guid invitationId, CancellationToken cancellationToken);

    Task<OrganizationResponse> AcceptInvitationAsync(string? userId, string token, AcceptInvitationRequest request, CancellationToken cancellationToken);
}
