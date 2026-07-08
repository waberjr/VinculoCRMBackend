using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;

public sealed record GetDocumentAttachmentsQuery(string EntityType, Guid EntityId) : IRequest<IReadOnlyCollection<DocumentAttachmentDto>>;

public sealed class GetDocumentAttachmentsQueryHandler : IRequestHandler<GetDocumentAttachmentsQuery, IReadOnlyCollection<DocumentAttachmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDocumentAttachmentsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<DocumentAttachmentDto>> Handle(GetDocumentAttachmentsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        return await _context.DocumentAttachments
            .AsNoTracking()
            .Where(document => document.EntityType == request.EntityType && document.EntityId == request.EntityId)
            .OrderByDescending(document => document.Created)
            .Select(document => new DocumentAttachmentDto(
                document.Id,
                document.EntityType,
                document.EntityId,
                document.Title,
                PublicUrl(document.Id, document.Url),
                document.Description,
                document.Created))
            .ToListAsync(cancellationToken);
    }

    private static string PublicUrl(Guid id, string url) =>
        url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase)
            ? $"/api/DocumentAttachments/{id}/Download"
            : url;
}
