using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Locations.Models;

namespace VinculoBackend.Application.Locations.Queries.GetStates;

public sealed record GetStatesQuery : IRequest<IReadOnlyCollection<StateDto>>;

public sealed class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, IReadOnlyCollection<StateDto>>
{
    private readonly ILocationLookupService _locationLookupService;

    public GetStatesQueryHandler(ILocationLookupService locationLookupService)
    {
        _locationLookupService = locationLookupService;
    }

    public Task<IReadOnlyCollection<StateDto>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        return _locationLookupService.GetStatesAsync(cancellationToken);
    }
}
