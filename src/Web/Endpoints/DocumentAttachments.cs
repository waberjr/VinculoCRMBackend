using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.UploadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Models;
using VinculoBackend.Application.DocumentAttachments.Queries.DownloadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;

namespace VinculoBackend.Web.Endpoints;

public sealed class DocumentAttachments : IEndpointGroup
{
    public sealed record CreateDocumentAttachmentRequest(string EntityType, Guid EntityId, string Title, string Url, string? Description);

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(List);
        groupBuilder.MapGet(Download, "{id}/Download");
        groupBuilder.MapPost(Create);
        groupBuilder.MapPost(Upload, "Upload").DisableAntiforgery();
        groupBuilder.MapDelete(Delete, "{id}");
    }

    public static async Task<Ok<IReadOnlyCollection<DocumentAttachmentDto>>> List(
        ISender sender,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetDocumentAttachmentsQuery(entityType, entityId), cancellationToken);
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

    public static async Task<NoContent> Delete(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDocumentAttachmentCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
