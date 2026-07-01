using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<bool>;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, bool>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<bool> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return _identityService.PasswordSignInAsync(request.Email, request.Password, cancellationToken);
    }
}
