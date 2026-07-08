using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;

public sealed record GetDocumentAttachmentsQuery(
    string? EntityType = null,
    Guid? EntityId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedResult<DocumentAttachmentDto>>;

public sealed class GetDocumentAttachmentsQueryHandler : IRequestHandler<GetDocumentAttachmentsQuery, PaginatedResult<DocumentAttachmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDocumentAttachmentsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<DocumentAttachmentDto>> Handle(GetDocumentAttachmentsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.DocumentAttachments
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(document => document.EntityType == request.EntityType.Trim());
        }

        if (request.EntityId is not null)
        {
            query = query.Where(document => document.EntityId == request.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(document =>
                document.Title.Contains(search) ||
                (document.Description != null && document.Description.Contains(search)));
        }

        var projected = query
            .OrderByDescending(document => document.Created)
            .Select(document => new DocumentAttachmentDto(
                document.Id,
                document.EntityType,
                document.EntityId,
                document.Title,
                PublicUrl(document.Id, document.Url),
                document.Description,
                document.Created,
                Kind(document.Url),
                document.OriginalFileName,
                document.ContentType,
                document.SizeBytes));

        return await PaginatedResult<DocumentAttachmentDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }

    private static string PublicUrl(Guid id, string url) =>
        url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase)
            ? $"/api/DocumentAttachments/{id}/Download"
            : url;

    private static string Kind(string url) =>
        url.StartsWith("storage://", StringComparison.OrdinalIgnoreCase) ? "File" : "Link";
}
