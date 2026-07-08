using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.ImpactUpdates.Commands.CreateImpactUpdate;
using VinculoBackend.Application.ImpactUpdates.Models;
using VinculoBackend.Application.ImpactUpdates.Queries.GetImpactUpdates;

namespace VinculoBackend.Web.Endpoints;

public sealed class ImpactUpdates : IEndpointGroup
{
    public sealed record CreateImpactUpdateRequest(Guid ProjectId, string Title, string Content);

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(List);
        groupBuilder.MapPost(Create);
    }

    public static async Task<Ok<IReadOnlyCollection<ImpactUpdateDto>>> List(
        ISender sender,
        Guid? projectId,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetImpactUpdatesQuery(projectId), cancellationToken);
        return TypedResults.Ok(items);
    }

    public static async Task<Created<Guid>> Create(
        ISender sender,
        CreateImpactUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CreateImpactUpdateCommand(request.ProjectId, request.Title, request.Content), cancellationToken);
        return TypedResults.Created($"/api/ImpactUpdates/{id}", id);
    }
}
