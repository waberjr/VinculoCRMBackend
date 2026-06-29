using VinculoBackend.Application.Locations.Models;
using VinculoBackend.Application.Locations.Queries.GetCities;
using VinculoBackend.Application.Locations.Queries.GetStates;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Locations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetStates, "States");
        groupBuilder.MapGet(GetCities, "States/{stateCode}/Cities");
    }

    public static async Task<Ok<IReadOnlyCollection<StateDto>>> GetStates(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStatesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<CityDto>>> GetCities(
        ISender sender,
        string stateCode,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCitiesQuery(stateCode), cancellationToken);
        return TypedResults.Ok(result);
    }
}
