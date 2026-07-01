using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Organizations.Commands.RevokeOrganizationInvitation;

public sealed record RevokeOrganizationInvitationCommand(Guid InvitationId) : IRequest;

public sealed class RevokeOrganizationInvitationCommandHandler : IRequestHandler<RevokeOrganizationInvitationCommand>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public RevokeOrganizationInvitationCommandHandler(IOrganizationAdministrationService service, IOrganizationContext organizationContext, IUser user)
    {
        _service = service;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task Handle(RevokeOrganizationInvitationCommand request, CancellationToken cancellationToken)
    {
        return _service.RevokeInvitationAsync(_user.Id ?? throw new UnauthorizedAccessException(), RequiredOrganization.From(_organizationContext), request.InvitationId, cancellationToken);
    }
}
