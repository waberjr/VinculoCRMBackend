using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Locations.Models;

namespace VinculoBackend.Application.Locations.Queries.GetCities;

public sealed record GetCitiesQuery(string StateCode) : IRequest<IReadOnlyCollection<CityDto>>;

public sealed class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, IReadOnlyCollection<CityDto>>
{
    private readonly ILocationLookupService _locationLookupService;

    public GetCitiesQueryHandler(ILocationLookupService locationLookupService)
    {
        _locationLookupService = locationLookupService;
    }

    public Task<IReadOnlyCollection<CityDto>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        return _locationLookupService.GetCitiesAsync(request.StateCode, cancellationToken);
    }
}
