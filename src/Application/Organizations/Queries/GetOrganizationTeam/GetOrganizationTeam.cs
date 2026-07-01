using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Organizations.Models;

namespace VinculoBackend.Application.Organizations.Queries.GetOrganizationTeam;

public sealed record GetOrganizationTeamQuery : IRequest<OrganizationTeamResponse>;

public sealed class GetOrganizationTeamQueryHandler : IRequestHandler<GetOrganizationTeamQuery, OrganizationTeamResponse>
{
    private readonly IOrganizationAdministrationService _service;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public GetOrganizationTeamQueryHandler(IOrganizationAdministrationService service, IOrganizationContext organizationContext, IUser user)
    {
        _service = service;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task<OrganizationTeamResponse> Handle(GetOrganizationTeamQuery request, CancellationToken cancellationToken)
    {
        return _service.GetTeamAsync(_user.Id ?? throw new UnauthorizedAccessException(), RequiredOrganization.From(_organizationContext), cancellationToken);
    }
}
