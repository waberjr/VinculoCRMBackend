using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Models;

namespace VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachmentAudit;

public sealed record GetDocumentAttachmentAuditQuery(
    Guid? DocumentAttachmentId = null,
    string? EntityType = null,
    Guid? EntityId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedResult<DocumentAttachmentAuditEntryDto>>;

public sealed class GetDocumentAttachmentAuditQueryHandler : IRequestHandler<GetDocumentAttachmentAuditQuery, PaginatedResult<DocumentAttachmentAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDocumentAttachmentAuditQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<DocumentAttachmentAuditEntryDto>> Handle(GetDocumentAttachmentAuditQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.DocumentAttachmentAuditEntries.AsNoTracking().AsQueryable();

        if (request.DocumentAttachmentId is not null)
        {
            query = query.Where(entry => entry.DocumentAttachmentId == request.DocumentAttachmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(entry => entry.EntityType == request.EntityType.Trim());
        }

        if (request.EntityId is not null)
        {
            query = query.Where(entry => entry.EntityId == request.EntityId.Value);
        }

        var projected = query
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Select(entry => new DocumentAttachmentAuditEntryDto
            {
                Id = entry.Id,
                DocumentAttachmentId = entry.DocumentAttachmentId,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                Title = entry.Title,
                CreatedByUserId = entry.CreatedByUserId,
                OccurredAtUtc = entry.OccurredAtUtc,
            });

        return await PaginatedResult<DocumentAttachmentAuditEntryDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
