using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(Guid Id, UpdateOrganizationRequest Request) : IRequest;

public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public UpdateOrganizationCommandHandler(IOrganizationAdministrationService service, IUser user)
    {
        _service = service;
        _user = user;
    }

    public Task Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        return _service.UpdateAsync(_user.Id ?? throw new UnauthorizedAccessException(), request.Id, request.Request, cancellationToken);
    }
}
