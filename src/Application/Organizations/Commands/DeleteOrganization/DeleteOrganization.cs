using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Organizations.Commands.DeleteOrganization;

public sealed record DeleteOrganizationCommand(Guid Id) : IRequest;

public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public DeleteOrganizationCommandHandler(IOrganizationAdministrationService service, IUser user)
    {
        _service = service;
        _user = user;
    }

    public Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        return _service.DeleteAsync(_user.Id ?? throw new UnauthorizedAccessException(), request.Id, cancellationToken);
    }
}
