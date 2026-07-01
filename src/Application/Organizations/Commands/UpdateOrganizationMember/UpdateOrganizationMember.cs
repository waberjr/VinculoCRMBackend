using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Commands.UpdateOrganizationMember;

public sealed record UpdateOrganizationMemberCommand(Guid MemberId, UpdateMemberRequest Request) : IRequest;

public sealed class UpdateOrganizationMemberCommandHandler : IRequestHandler<UpdateOrganizationMemberCommand>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public UpdateOrganizationMemberCommandHandler(IOrganizationAdministrationService service, IOrganizationContext organizationContext, IUser user)
    {
        _service = service;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task Handle(UpdateOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        return _service.UpdateMemberAsync(_user.Id ?? throw new UnauthorizedAccessException(), RequiredOrganization.From(_organizationContext), request.MemberId, request.Request, cancellationToken);
    }
}
