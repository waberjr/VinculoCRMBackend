namespace VinculoBackend.Application.Organizations.Models;

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
