using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.DocumentAttachments.Queries.CreateDocumentAttachmentAccessUrl;

public sealed record CreateDocumentAttachmentAccessUrlQuery(Guid Id, int Minutes = 15) : IRequest<DocumentAttachmentAccessUrlDto?>;

public sealed class CreateDocumentAttachmentAccessUrlQueryHandler : IRequestHandler<CreateDocumentAttachmentAccessUrlQuery, DocumentAttachmentAccessUrlDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;

    public CreateDocumentAttachmentAccessUrlQueryHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
    }

    public async Task<DocumentAttachmentAccessUrlDto?> Handle(CreateDocumentAttachmentAccessUrlQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var document = await _context.DocumentAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (!document.Url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            return new DocumentAttachmentAccessUrlDto(document.Url, DateTimeOffset.UtcNow.AddMinutes(request.Minutes));
        }

        var ttl = TimeSpan.FromMinutes(Math.Clamp(request.Minutes, 1, 60));
        var accessUrl = await _fileStorage.CreateTemporaryReadUrlAsync(document.Url, ttl, cancellationToken);
        return accessUrl is null ? null : new DocumentAttachmentAccessUrlDto(accessUrl.Url, accessUrl.ExpiresAtUtc);
    }
}
