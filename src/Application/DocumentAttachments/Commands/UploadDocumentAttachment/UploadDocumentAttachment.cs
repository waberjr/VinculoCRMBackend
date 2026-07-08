using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Options;
using Microsoft.Extensions.Options;
using FluentValidation.Results;

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
    private readonly DocumentUploadOptions _options;

    public UploadDocumentAttachmentCommandHandler(
        IFileStorageService fileStorage,
        ISender sender,
        IOrganizationContext organizationContext,
        IOptions<DocumentUploadOptions> options)
    {
        _fileStorage = fileStorage;
        _sender = sender;
        _organizationContext = organizationContext;
        _options = options.Value;
    }

    public async Task<Guid> Handle(UploadDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        ValidateFile(request);

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
                request.Description,
                request.FileName,
                request.ContentType,
                request.Length),
            cancellationToken);
    }

    private void ValidateFile(UploadDocumentAttachmentCommand request)
    {
        var failures = new List<ValidationFailure>();
        if (request.Length <= 0)
        {
            failures.Add(new ValidationFailure("File", "Informe um arquivo valido."));
        }

        if (request.Length > _options.MaxFileSizeBytes)
        {
            failures.Add(new ValidationFailure("File", $"O arquivo deve ter no maximo {FormatBytes(_options.MaxFileSizeBytes)}."));
        }

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure("FileName", "Tipo de arquivo nao permitido."));
        }

        if (string.IsNullOrWhiteSpace(request.ContentType) ||
            !_options.AllowedContentTypes.Contains(request.ContentType.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure("ContentType", "Conteudo do arquivo nao permitido."));
        }

        if (failures.Count > 0)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException(failures);
        }
    }

    private static string FormatBytes(long bytes)
    {
        const long megabyte = 1024 * 1024;
        return bytes >= megabyte ? $"{bytes / megabyte} MB" : $"{bytes} bytes";
    }
}
