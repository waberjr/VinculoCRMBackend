using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Organizations.Commands.DeleteOrganizationMember;

public sealed record DeleteOrganizationMemberCommand(Guid MemberId) : IRequest;

public sealed class DeleteOrganizationMemberCommandHandler : IRequestHandler<DeleteOrganizationMemberCommand>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public DeleteOrganizationMemberCommandHandler(IOrganizationAdministrationService service, IOrganizationContext organizationContext, IUser user)
    {
        _service = service;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task Handle(DeleteOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        return _service.DeleteMemberAsync(_user.Id ?? throw new UnauthorizedAccessException(), RequiredOrganization.From(_organizationContext), request.MemberId, cancellationToken);
    }
}
