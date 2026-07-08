using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.DocumentAttachments.Commands.UploadDocumentAttachment;

public sealed record UploadDocumentAttachmentCommand(
    string EntityType,
    Guid EntityId,
    string Title,
    string? Description,
    string FileName,
    string ContentType,
    Stream Content,
    long Length) : IRequest<Guid>;

public sealed class UploadDocumentAttachmentCommandHandler : IRequestHandler<UploadDocumentAttachmentCommand, Guid>
{
    private readonly IFileStorageService _fileStorage;
    private readonly ISender _sender;
    private readonly IOrganizationContext _organizationContext;

    public UploadDocumentAttachmentCommandHandler(IFileStorageService fileStorage, ISender sender, IOrganizationContext organizationContext)
    {
        _fileStorage = fileStorage;
        _sender = sender;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(UploadDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure("File", "Informe um arquivo valido.")]);
        }

        var organizationId = RequiredOrganization.From(_organizationContext);
        var storedFile = await _fileStorage.StoreAsync(
            new StoreFileRequest(
                organizationId,
                request.EntityType.Trim(),
                request.EntityId,
                string.IsNullOrWhiteSpace(request.FileName) ? request.Title.Trim() : request.FileName,
                request.ContentType,
                request.Content,
                request.Length),
            cancellationToken);

        return await _sender.Send(
            new CreateDocumentAttachment.CreateDocumentAttachmentCommand(
                request.EntityType,
                request.EntityId,
                request.Title,
                storedFile.InternalUri,
                request.Description),
            cancellationToken);
    }
}
