using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Users.Models;

namespace VinculoBackend.Application.Users.Queries.GetAttendants;

public record GetAttendantsQuery : IRequest<IReadOnlyCollection<AttendantDto>>;

public sealed class GetAttendantsQueryHandler : IRequestHandler<GetAttendantsQuery, IReadOnlyCollection<AttendantDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public GetAttendantsQueryHandler(IIdentityService identityService, IOrganizationContext organizationContext, IUser user)
    {
        _identityService = identityService;
        _organizationContext = organizationContext;
        _user = user;
    }

    public Task<IReadOnlyCollection<AttendantDto>> Handle(GetAttendantsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        return string.IsNullOrWhiteSpace(_user.Id)
            ? Task.FromResult<IReadOnlyCollection<AttendantDto>>([])
            : _identityService.GetAttendantsAsync(_user.Id, organizationId, cancellationToken);
    }
}
