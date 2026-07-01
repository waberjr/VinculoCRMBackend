using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Commands.AcceptOrganizationInvitation;

public sealed record AcceptOrganizationInvitationCommand(string Token, AcceptInvitationRequest Request) : IRequest<OrganizationResponse>;

public sealed class AcceptOrganizationInvitationCommandHandler : IRequestHandler<AcceptOrganizationInvitationCommand, OrganizationResponse>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public AcceptOrganizationInvitationCommandHandler(IOrganizationAdministrationService service, IUser user)
    {
        _service = service;
        _user = user;
    }

    public Task<OrganizationResponse> Handle(AcceptOrganizationInvitationCommand request, CancellationToken cancellationToken)
    {
        return _service.AcceptInvitationAsync(_user.Id, request.Token, request.Request, cancellationToken);
    }
}
