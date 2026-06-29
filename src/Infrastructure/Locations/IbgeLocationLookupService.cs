using System.Net.Http.Json;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Locations.Models;

namespace VinculoBackend.Infrastructure.Locations;

public sealed class IbgeLocationLookupService : ILocationLookupService
{
    private readonly HttpClient _httpClient;

    public IbgeLocationLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<StateDto>> GetStatesAsync(CancellationToken cancellationToken)
    {
        var states = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<IbgeState>>(
            "localidades/estados?orderBy=nome",
            cancellationToken) ?? [];

        return states
            .OrderBy(state => state.Nome)
            .Select(state => new StateDto(state.Id, state.Sigla, state.Nome))
            .ToList();
    }

    public async Task<IReadOnlyCollection<CityDto>> GetCitiesAsync(string stateCode, CancellationToken cancellationToken)
    {
        var cities = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<IbgeCity>>(
            $"localidades/estados/{Uri.EscapeDataString(stateCode)}/municipios?orderBy=nome",
            cancellationToken) ?? [];

        return cities
            .OrderBy(city => city.Nome)
            .Select(city => new CityDto(city.Id, city.Nome))
            .ToList();
    }

    private sealed record IbgeState(int Id, string Sigla, string Nome);

    private sealed record IbgeCity(int Id, string Nome);
}
