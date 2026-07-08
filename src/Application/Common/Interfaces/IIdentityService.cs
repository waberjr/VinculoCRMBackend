using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Users.Models;
using System.Security.Claims;

namespace VinculoBackend.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<ClaimsPrincipal?> PasswordSignInAsync(string email, string password, CancellationToken cancellationToken);

    Task<ClaimsPrincipal?> RefreshSignInPrincipalAsync(string refreshToken, CancellationToken cancellationToken);

    Task<CurrentUserDto?> GetCurrentUserAsync(string userId, Guid? requestedOrganizationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AttendantDto>> GetAttendantsAsync(string userId, Guid organizationId, CancellationToken cancellationToken);

    Task<Result> DeleteUserAsync(string userId);
}
