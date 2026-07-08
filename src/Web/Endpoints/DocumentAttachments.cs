using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.UploadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Models;
using VinculoBackend.Application.DocumentAttachments.Queries.CreateDocumentAttachmentAccessUrl;
using VinculoBackend.Application.DocumentAttachments.Queries.DownloadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachmentAudit;
using VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Web.Endpoints;

public sealed class DocumentAttachments : IEndpointGroup
{
    public sealed record CreateDocumentAttachmentRequest(string EntityType, Guid EntityId, string Title, string Url, string? Description);

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(List);
        groupBuilder.MapGet(Audit, "Audit");
        groupBuilder.MapGet(Download, "{id}/Download");
        groupBuilder.MapGet(AccessUrl, "{id}/AccessUrl");
        groupBuilder.MapPost(Create);
        groupBuilder.MapPost(Upload, "Upload").DisableAntiforgery();
        groupBuilder.MapDelete(Delete, "{id}");
    }

    public static async Task<Ok<PaginatedResult<DocumentAttachmentDto>>> List(
        ISender sender,
        string? entityType,
        Guid? entityId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetDocumentAttachmentsQuery(entityType, entityId, search, pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize), cancellationToken);
        return TypedResults.Ok(items);
    }

    public static async Task<Created<Guid>> Create(
        ISender sender,
        CreateDocumentAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateDocumentAttachmentCommand(
                request.EntityType,
                request.EntityId,
                request.Title,
                request.Url,
                request.Description),
            cancellationToken);

        return TypedResults.Created($"/api/DocumentAttachments/{id}", id);
    }

    public static async Task<Ok<PaginatedResult<DocumentAttachmentAuditEntryDto>>> Audit(
        ISender sender,
        Guid? documentAttachmentId,
        string? entityType,
        Guid? entityId,
        string? action,
        string? createdByUserId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(
            new GetDocumentAttachmentAuditQuery(
                documentAttachmentId,
                entityType,
                entityId,
                action,
                createdByUserId,
                startDateUtc,
                endDateUtc,
                pageNumber <= 0 ? 1 : pageNumber,
                pageSize <= 0 ? 20 : pageSize),
            cancellationToken);

        return TypedResults.Ok(items);
    }

    public static async Task<Created<Guid>> Upload(
        ISender sender,
        IFormFile file,
        string entityType,
        Guid entityId,
        string title,
        string? description,
        CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        var id = await sender.Send(
            new UploadDocumentAttachmentCommand(
                entityType,
                entityId,
                title,
                description,
                file.FileName,
                file.ContentType,
                content,
                file.Length),
            cancellationToken);

        return TypedResults.Created($"/api/DocumentAttachments/{id}", id);
    }

    public static async Task<Results<FileStreamHttpResult, NotFound>> Download(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken)
    {
        var download = await sender.Send(new DownloadDocumentAttachmentQuery(id), cancellationToken);
        return download is null
            ? TypedResults.NotFound()
            : TypedResults.File(download.Content, download.ContentType, download.FileName);
    }

    public static async Task<Results<Ok<DocumentAttachmentAccessUrlDto>, NotFound>> AccessUrl(
        ISender sender,
        Guid id,
        int minutes,
        CancellationToken cancellationToken)
    {
        var accessUrl = await sender.Send(new CreateDocumentAttachmentAccessUrlQuery(id, minutes <= 0 ? 15 : minutes), cancellationToken);
        return accessUrl is null ? TypedResults.NotFound() : TypedResults.Ok(accessUrl);
    }

    public static async Task<NoContent> Delete(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDocumentAttachmentCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
