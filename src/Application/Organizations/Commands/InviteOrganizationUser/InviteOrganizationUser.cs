using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Commands.InviteOrganizationUser;

public sealed record InviteOrganizationUserCommand(InviteUserRequest Request) : IRequest<InviteUserResponse>;

public sealed class InviteOrganizationUserCommandHandler : IRequestHandler<InviteOrganizationUserCommand, InviteUserResponse>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public InviteOrganizationUserCommandHandler(IOrganizationAdministrationService service, IOrganizationContext organizationContext, IUser user)
    {
        _service = service;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task<InviteUserResponse> Handle(InviteOrganizationUserCommand request, CancellationToken cancellationToken)
    {
        var activeOrganizationId = _organizationContext.HasOrganization ? _organizationContext.OrganizationId : (Guid?)null;
        return _service.InviteAsync(_user.Id ?? throw new UnauthorizedAccessException(), activeOrganizationId, request.Request, cancellationToken);
    }
}
