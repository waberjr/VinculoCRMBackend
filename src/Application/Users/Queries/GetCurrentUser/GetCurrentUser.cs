using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Users.Models;

namespace VinculoBackend.Application.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto?>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public GetCurrentUserQueryHandler(IIdentityService identityService, IOrganizationContext organizationContext, IUser user)
    {
        _identityService = identityService;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(_user.Id)
            ? Task.FromResult<CurrentUserDto?>(null)
            : _identityService.GetCurrentUserAsync(
                _user.Id,
                _organizationContext.HasOrganization ? _organizationContext.OrganizationId : null,
                cancellationToken);
    }
}
