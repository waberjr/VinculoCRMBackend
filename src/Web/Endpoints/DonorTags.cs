using VinculoBackend.Application.DonorTags.Commands.CreateDonorTag;
using VinculoBackend.Application.DonorTags.Models;
using VinculoBackend.Application.DonorTags.Queries.GetDonorTags;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class DonorTags : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetDonorTags);
        groupBuilder.MapPost(CreateDonorTag);
    }

    public static async Task<Ok<IReadOnlyCollection<DonorTagDto>>> GetDonorTags(ISender sender, bool includeInactive = false)
    {
        var result = await sender.Send(new GetDonorTagsQuery(includeInactive));
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateDonorTag(ISender sender, CreateDonorTagCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/DonorTags/{id}", id);
    }
}
