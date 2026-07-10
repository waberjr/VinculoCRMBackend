using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.ImpactProjects.Commands.CreateProject;
using VinculoBackend.Application.ImpactProjects.Commands.UpdateProject;
using VinculoBackend.Application.ImpactProjects.Models;
using VinculoBackend.Application.ImpactProjects.Queries.ExportProjectAccountability;
using VinculoBackend.Application.ImpactProjects.Queries.GetProjectAccountability;
using VinculoBackend.Application.ImpactProjects.Queries.GetProjects;

namespace VinculoBackend.Web.Endpoints;

public sealed class Projects : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetProjects);
        groupBuilder.MapGet(Accountability, "{id}/Accountability");
        groupBuilder.MapGet(AccountabilityExport, "{id}/Accountability/Export");
        groupBuilder.MapPost(CreateProject);
        groupBuilder.MapPut(UpdateProject, "{id}");
    }

    public static async Task<Results<FileContentHttpResult, NotFound>> AccountabilityExport(
        ISender sender,
        Guid id,
        string? format,
        Guid? campaignId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportProjectAccountabilityQuery(id, format ?? "csv", campaignId, startDateUtc, endDateUtc), cancellationToken);
        return result is null
            ? TypedResults.NotFound()
            : TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<Results<Ok<ProjectAccountabilityDto>, NotFound>> Accountability(
        ISender sender,
        Guid id,
        Guid? campaignId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProjectAccountabilityQuery(id, campaignId, startDateUtc, endDateUtc), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<PaginatedResult<ProjectListItemDto>>> GetProjects(
        ISender sender,
        string? search,
        string? status,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetProjectsQuery
        {
            Search = search,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateProject(ISender sender, CreateProjectCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Projects/{id}", id);
    }

    public static async Task<NoContent> UpdateProject(ISender sender, Guid id, UpdateProjectCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }
}
