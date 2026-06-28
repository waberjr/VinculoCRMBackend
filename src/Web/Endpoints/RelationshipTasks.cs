using VinculoBackend.Application.RelationshipTasks.Commands.CancelRelationshipTask;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.RelationshipTasks.Commands.CompleteRelationshipTask;
using VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;
using VinculoBackend.Application.RelationshipTasks.Models;
using VinculoBackend.Application.RelationshipTasks.Queries.GetRelationshipTasks;
using VinculoBackend.Application.RelationshipTasks.Commands.StartRelationshipTask;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class RelationshipTasks : IEndpointGroup
{
    public static string RoutePrefix => "/api/Tasks";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetTasks);
        groupBuilder.MapPost(CreateTask);
        groupBuilder.MapPost(StartTask, "{id}/Start");
        groupBuilder.MapPost(CompleteTask, "{id}/Complete");
        groupBuilder.MapPost(CancelTask, "{id}/Cancel");
    }

    public static async Task<Ok<PaginatedResult<RelationshipTaskListItemDto>>> GetTasks(
        ISender sender,
        string? search,
        Guid? donorId,
        Guid? campaignId,
        Guid? statusOptionId,
        Guid? priorityOptionId,
        string? assignedUserId,
        DateTimeOffset? dueFromUtc,
        DateTimeOffset? dueToUtc,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetRelationshipTasksQuery
        {
            Search = search,
            DonorId = donorId,
            CampaignId = campaignId,
            StatusOptionId = statusOptionId,
            PriorityOptionId = priorityOptionId,
            AssignedUserId = assignedUserId,
            DueFromUtc = dueFromUtc,
            DueToUtc = dueToUtc,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateTask(ISender sender, CreateRelationshipTaskCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Tasks/{id}", id);
    }

    public static async Task<NoContent> CompleteTask(ISender sender, Guid id, CompleteRelationshipTaskCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> StartTask(ISender sender, Guid id)
    {
        await sender.Send(new StartRelationshipTaskCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelTask(ISender sender, Guid id)
    {
        await sender.Send(new CancelRelationshipTaskCommand(id));
        return TypedResults.NoContent();
    }
}
