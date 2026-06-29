using VinculoBackend.Application.Locations.Models;

namespace VinculoBackend.Application.Common.Interfaces;

public interface ILocationLookupService
{
    Task<IReadOnlyCollection<StateDto>> GetStatesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CityDto>> GetCitiesAsync(string stateCode, CancellationToken cancellationToken);
}
