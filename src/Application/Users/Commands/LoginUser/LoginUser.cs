using VinculoBackend.Application.Common.Interfaces;
using System.Security.Claims;

namespace VinculoBackend.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<ClaimsPrincipal?>;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ClaimsPrincipal?>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<ClaimsPrincipal?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return _identityService.PasswordSignInAsync(request.Email, request.Password, cancellationToken);
    }
}
