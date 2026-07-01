using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Queries.ListOrganizations;

public sealed record ListOrganizationsQuery : IRequest<IReadOnlyCollection<OrganizationResponse>>;

public sealed class ListOrganizationsQueryHandler : IRequestHandler<ListOrganizationsQuery, IReadOnlyCollection<OrganizationResponse>>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public ListOrganizationsQueryHandler(IOrganizationAdministrationService service, IUser user)
    {
        _service = service;
        _user = user;
    }

    public Task<IReadOnlyCollection<OrganizationResponse>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
    {
        return _service.ListAsync(_user.Id ?? throw new UnauthorizedAccessException(), cancellationToken);
    }
}
