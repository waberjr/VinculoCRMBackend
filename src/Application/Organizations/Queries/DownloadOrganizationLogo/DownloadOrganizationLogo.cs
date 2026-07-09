using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.Organizations.Queries.DownloadOrganizationLogo;

public sealed record DownloadOrganizationLogoQuery(Guid OrganizationId) : IRequest<DocumentAttachmentDownloadDto?>;

public sealed class DownloadOrganizationLogoQueryHandler : IRequestHandler<DownloadOrganizationLogoQuery, DocumentAttachmentDownloadDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public DownloadOrganizationLogoQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<DocumentAttachmentDownloadDto?> Handle(DownloadOrganizationLogoQuery request, CancellationToken cancellationToken)
    {
        var logoUrl = await _context.Organizations
            .AsNoTracking()
            .Where(organization => organization.Id == request.OrganizationId && organization.IsActive)
            .Select(organization => organization.LogoUrl)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(logoUrl) || !logoUrl.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var download = await _fileStorage.OpenReadAsync(logoUrl, cancellationToken);
        return download is null
            ? null
            : new DocumentAttachmentDownloadDto(download.FileName, download.ContentType, download.Content);
    }
}
