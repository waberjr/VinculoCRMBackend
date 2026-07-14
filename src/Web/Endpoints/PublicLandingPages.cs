using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetPublicLandingPage;

namespace VinculoBackend.Web.Endpoints;

public sealed class PublicLandingPages : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetLanding, "{kind}/{id}");
    }

    public static async Task<Results<Ok<PublicLandingPageDto>, NotFound>> GetLanding(
        ISender sender,
        string kind,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicLandingPageQuery(kind, id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
