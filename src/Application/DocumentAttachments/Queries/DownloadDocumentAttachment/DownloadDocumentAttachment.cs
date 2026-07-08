using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.DocumentAttachments.Queries.DownloadDocumentAttachment;

public sealed record DownloadDocumentAttachmentQuery(Guid Id) : IRequest<DocumentAttachmentDownloadDto?>;

public sealed class DownloadDocumentAttachmentQueryHandler : IRequestHandler<DownloadDocumentAttachmentQuery, DocumentAttachmentDownloadDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;

    public DownloadDocumentAttachmentQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage, IOrganizationContext organizationContext)
    {
        _context = context;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
    }

    public async Task<DocumentAttachmentDownloadDto?> Handle(DownloadDocumentAttachmentQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var document = await _context.DocumentAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (document is null || !document.Url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var download = await _fileStorage.OpenReadAsync(document.Url, cancellationToken);
        return download is null ? null : new DocumentAttachmentDownloadDto(download.FileName, download.ContentType, download.Content);
    }
}
