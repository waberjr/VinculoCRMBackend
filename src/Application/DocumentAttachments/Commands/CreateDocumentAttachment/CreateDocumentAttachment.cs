using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Options;
using VinculoBackend.Application.DocumentAttachments.Services;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;

public sealed record CreateDocumentAttachmentCommand(
    string EntityType,
    Guid EntityId,
    string Title,
    string? Url,
    string? Description,
    FileUpload? File = null,
    string? OriginalFileName = null,
    string? ContentType = null,
    long? SizeBytes = null) : IRequest<Guid>;

public sealed class CreateDocumentAttachmentCommandHandler : IRequestHandler<CreateDocumentAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;
    private readonly IDocumentAttachmentAuditService _auditService;
    private readonly DocumentUploadOptions _options;
    private readonly IUser _user;

    public CreateDocumentAttachmentCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        IOrganizationContext organizationContext,
        IDocumentAttachmentAuditService auditService,
        IOptions<DocumentUploadOptions> options,
        IUser user)
    {
        _context = context;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
        _auditService = auditService;
        _options = options.Value;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Title))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure("Document", "Informe entidade, titulo e link do documento.")]);
        }

        if (!await DocumentAttachmentEntityLookup.EntityExistsAsync(_context, request.EntityType, request.EntityId, cancellationToken))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(request.EntityType, request.EntityId.ToString());
        }

        var organizationId = RequiredOrganization.From(_organizationContext);
        var url = request.Url;
        var originalFileName = request.OriginalFileName;
        var contentType = request.ContentType;
        var sizeBytes = request.SizeBytes;

        if (request.File is not null)
        {
            ValidateFile(request.File);
            var storedFile = await _fileStorage.StoreAsync(
                new StoreFileRequest(
                    organizationId,
                    request.EntityType.Trim(),
                    request.EntityId,
                    string.IsNullOrWhiteSpace(request.File.FileName) ? request.Title.Trim() : request.File.FileName,
                    request.File.ContentType,
                    request.File.Content,
                    request.File.Length),
                cancellationToken);

            url = storedFile.InternalUri;
            originalFileName = request.File.FileName;
            contentType = request.File.ContentType;
            sizeBytes = request.File.Length;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure("Document", "Informe um link ou arquivo para o documento.")]);
        }

        var document = new DocumentAttachment
        {
            OrganizationId = organizationId,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            Title = request.Title.Trim(),
            Url = url.Trim(),
            Description = request.Description?.Trim(),
            OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? null : originalFileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim(),
            SizeBytes = sizeBytes,
            CreatedByUserId = _user.Id,
        };

        _context.DocumentAttachments.Add(document);
        await _auditService.RecordAsync(document, "Created", "Documento vinculado", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }

    private void ValidateFile(FileUpload file)
    {
        var failures = new List<ValidationFailure>();
        if (file.Length <= 0)
        {
            failures.Add(new ValidationFailure("File", "Informe um arquivo valido."));
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            failures.Add(new ValidationFailure("File", $"O arquivo deve ter no maximo {FormatBytes(_options.MaxFileSizeBytes)}."));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure("FileName", "Tipo de arquivo nao permitido."));
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !_options.AllowedContentTypes.Contains(file.ContentType.Trim(), StringComparer.OrdinalIgnoreCase))
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
