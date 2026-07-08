using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Security;
using VinculoBackend.Application.DocumentAttachments.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;

[Authorize]
public sealed record DeleteDocumentAttachmentCommand(Guid Id) : IRequest;

public sealed class DeleteDocumentAttachmentCommandHandler : IRequestHandler<DeleteDocumentAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentAttachmentAuditService _auditService;
    private readonly IDocumentAttachmentAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;

    public DeleteDocumentAttachmentCommandHandler(
        IApplicationDbContext context,
        IDocumentAttachmentAuditService auditService,
        IDocumentAttachmentAuthorizationService authorizationService,
        IFileStorageService fileStorage,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _auditService = auditService;
        _authorizationService = authorizationService;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
    }

    public async Task Handle(DeleteDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var document = await _context.DocumentAttachments.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(DocumentAttachment), request.Id.ToString());

        if (!_authorizationService.CanDelete(document))
        {
            throw new ForbiddenAccessException();
        }

        if (document.Url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            await _fileStorage.DeleteAsync(document.Url, cancellationToken);
        }

        await _auditService.RecordAsync(document, "Deleted", "Documento removido", cancellationToken);
        _context.DocumentAttachments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
