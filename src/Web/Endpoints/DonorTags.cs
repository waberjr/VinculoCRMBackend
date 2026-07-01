using VinculoBackend.Application.DonorTags.Commands.CreateDonorTag;
using VinculoBackend.Application.DonorTags.Commands.DeleteDonorTag;
using VinculoBackend.Application.DonorTags.Commands.UpdateDonorTag;
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
        groupBuilder.MapPut(UpdateDonorTag, "{id}");
        groupBuilder.MapDelete(DeleteDonorTag, "{id}");
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

    public static async Task<NoContent> UpdateDonorTag(ISender sender, Guid id, UpdateDonorTagCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteDonorTag(ISender sender, Guid id)
    {
        await sender.Send(new DeleteDonorTagCommand(id));
        return TypedResults.NoContent();
    }
}
