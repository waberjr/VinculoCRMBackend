using System.Security.Claims;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Users.Commands.RefreshUserToken;

public record RefreshUserTokenCommand(string RefreshToken) : IRequest<ClaimsPrincipal?>;

public sealed class RefreshUserTokenCommandHandler : IRequestHandler<RefreshUserTokenCommand, ClaimsPrincipal?>
{
    private readonly IIdentityService _identityService;

    public RefreshUserTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<ClaimsPrincipal?> Handle(RefreshUserTokenCommand request, CancellationToken cancellationToken)
    {
        return _identityService.RefreshSignInPrincipalAsync(request.RefreshToken, cancellationToken);
    }
}
