using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Security;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;

[Authorize(Roles = Roles.SystemAdministrator + "," + Roles.Administrator + "," + Roles.Manager)]
public sealed record DeleteDocumentAttachmentCommand(Guid Id) : IRequest;

public sealed class DeleteDocumentAttachmentCommandHandler : IRequestHandler<DeleteDocumentAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;

    public DeleteDocumentAttachmentCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IOrganizationContext organizationContext)
    {
        _context = context;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
    }

    public async Task Handle(DeleteDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var document = await _context.DocumentAttachments.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(DocumentAttachment), request.Id.ToString());

        if (document.Url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            await _fileStorage.DeleteAsync(document.Url, cancellationToken);
        }

        _context.DocumentAttachments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
