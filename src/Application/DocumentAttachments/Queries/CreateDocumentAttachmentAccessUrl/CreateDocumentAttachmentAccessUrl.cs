using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;
using VinculoBackend.Application.DocumentAttachments.Services;

namespace VinculoBackend.Application.DocumentAttachments.Queries.CreateDocumentAttachmentAccessUrl;

public sealed record CreateDocumentAttachmentAccessUrlQuery(Guid Id, int Minutes = 15) : IRequest<DocumentAttachmentAccessUrlDto?>;

public sealed class CreateDocumentAttachmentAccessUrlQueryHandler : IRequestHandler<CreateDocumentAttachmentAccessUrlQuery, DocumentAttachmentAccessUrlDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentAttachmentAuditService _auditService;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public CreateDocumentAttachmentAccessUrlQueryHandler(
        IApplicationDbContext context,
        IDocumentAttachmentAuditService auditService,
        IFileStorageService fileStorage,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _auditService = auditService;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<DocumentAttachmentAccessUrlDto?> Handle(CreateDocumentAttachmentAccessUrlQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var document = await _context.DocumentAttachments
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (!document.Url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            await _auditService.RecordAsync(document, "AccessUrlGenerated", "Link temporario de documento gerado", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new DocumentAttachmentAccessUrlDto(document.Url, _timeProvider.GetUtcNow().AddMinutes(ClampMinutes(request.Minutes)));
        }

        var ttl = TimeSpan.FromMinutes(ClampMinutes(request.Minutes));
        var accessUrl = await _fileStorage.CreateTemporaryReadUrlAsync(document.Url, ttl, cancellationToken);
        if (accessUrl is null)
        {
            return null;
        }

        await _auditService.RecordAsync(document, "AccessUrlGenerated", "Link temporario de documento gerado", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new DocumentAttachmentAccessUrlDto(accessUrl.Url, accessUrl.ExpiresAtUtc);
    }

    private static int ClampMinutes(int minutes) => Math.Clamp(minutes, 1, 60);
}
