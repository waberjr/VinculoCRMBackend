using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace VinculoBackend.Web.Endpoints;

public sealed class ImpactUpdates : IEndpointGroup
{
    public sealed record ImpactUpdateDto(Guid Id, Guid ProjectId, string Title, string Content, DateTimeOffset PublishedAtUtc);
    public sealed record CreateImpactUpdateRequest(Guid ProjectId, string Title, string Content);

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(List);
        groupBuilder.MapPost(Create);
    }

    public static async Task<Ok<IReadOnlyCollection<ImpactUpdateDto>>> List(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        Guid? projectId,
        CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(organizationContext);
        var query = context.ImpactUpdates.AsNoTracking();
        if (projectId is not null)
        {
            query = query.Where(update => update.ProjectId == projectId);
        }

        var items = await query
            .OrderByDescending(update => update.PublishedAtUtc)
            .Take(50)
            .Select(update => new ImpactUpdateDto(update.Id, update.ProjectId, update.Title, update.Content, update.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok((IReadOnlyCollection<ImpactUpdateDto>)items);
    }

    public static async Task<Results<Created<Guid>, NotFound>> Create(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        CreateImpactUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(organizationContext);
        var projectExists = await context.Projects.AsNoTracking().AnyAsync(project => project.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return TypedResults.NotFound();
        }

        var update = new ImpactUpdate
        {
            OrganizationId = organizationId,
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            PublishedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = user.Id,
        };

        context.ImpactUpdates.Add(update);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/ImpactUpdates/{update.Id}", update.Id);
    }
}
