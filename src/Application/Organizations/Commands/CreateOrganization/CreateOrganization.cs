using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(CreateOrganizationRequest Request) : IRequest<OrganizationResponse>;

public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResponse>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public CreateOrganizationCommandHandler(IOrganizationAdministrationService service, IUser user)
    {
        _service = service;
        _user = user;
    }

    public Task<OrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        return _service.CreateAsync(_user.Id ?? throw new UnauthorizedAccessException(), request.Request, cancellationToken);
    }
}
