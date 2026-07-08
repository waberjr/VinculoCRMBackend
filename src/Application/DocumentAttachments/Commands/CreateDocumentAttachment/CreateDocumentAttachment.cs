using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;

public sealed record CreateDocumentAttachmentCommand(
    string EntityType,
    Guid EntityId,
    string Title,
    string Url,
    string? Description,
    string? OriginalFileName = null,
    string? ContentType = null,
    long? SizeBytes = null) : IRequest<Guid>;

public sealed class CreateDocumentAttachmentCommandHandler : IRequestHandler<CreateDocumentAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IDocumentAttachmentAuditService _auditService;
    private readonly IUser _user;

    public CreateDocumentAttachmentCommandHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IDocumentAttachmentAuditService auditService,
        IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _auditService = auditService;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Url))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure("Document", "Informe entidade, titulo e link do documento.")]);
        }

        if (!await DocumentAttachmentEntityLookup.EntityExistsAsync(_context, request.EntityType, request.EntityId, cancellationToken))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(request.EntityType, request.EntityId.ToString());
        }

        var organizationId = RequiredOrganization.From(_organizationContext);
        var document = new DocumentAttachment
        {
            OrganizationId = organizationId,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            Title = request.Title.Trim(),
            Url = request.Url.Trim(),
            Description = request.Description?.Trim(),
            OriginalFileName = string.IsNullOrWhiteSpace(request.OriginalFileName) ? null : request.OriginalFileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            CreatedByUserId = _user.Id,
        };

        _context.DocumentAttachments.Add(document);
        await _auditService.RecordAsync(document, "Created", "Documento vinculado", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
