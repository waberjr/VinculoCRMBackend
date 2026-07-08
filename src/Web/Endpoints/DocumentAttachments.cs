using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace VinculoBackend.Web.Endpoints;

public sealed class DocumentAttachments : IEndpointGroup
{
    public sealed record DocumentAttachmentDto(Guid Id, string EntityType, Guid EntityId, string Title, string Url, string? Description, DateTimeOffset Created);
    public sealed record CreateDocumentAttachmentRequest(string EntityType, Guid EntityId, string Title, string Url, string? Description);

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(List);
        groupBuilder.MapPost(Create);
    }

    public static async Task<Ok<IReadOnlyCollection<DocumentAttachmentDto>>> List(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(organizationContext);

        var items = await context.DocumentAttachments
            .AsNoTracking()
            .Where(document => document.EntityType == entityType && document.EntityId == entityId)
            .OrderByDescending(document => document.Created)
            .Select(document => new DocumentAttachmentDto(
                document.Id,
                document.EntityType,
                document.EntityId,
                document.Title,
                document.Url,
                document.Description,
                document.Created))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok((IReadOnlyCollection<DocumentAttachmentDto>)items);
    }

    public static async Task<Created<Guid>> Create(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        CreateDocumentAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(organizationContext);
        var document = new DocumentAttachment
        {
            OrganizationId = organizationId,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            Title = request.Title.Trim(),
            Url = request.Url.Trim(),
            Description = request.Description?.Trim(),
            CreatedByUserId = user.Id,
        };

        context.DocumentAttachments.Add(document);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Created($"/api/DocumentAttachments/{document.Id}", document.Id);
    }
}
