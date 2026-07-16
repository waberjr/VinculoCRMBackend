using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageAudit;

public sealed record GetLandingPageAuditQuery(
    string? EntityType = null,
    Guid? EntityId = null,
    string? Action = null,
    int Limit = 50) : IRequest<IReadOnlyCollection<LandingPageAuditEntryDto>>;

public sealed class GetLandingPageAuditQueryHandler : IRequestHandler<GetLandingPageAuditQuery, IReadOnlyCollection<LandingPageAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageAuditQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<LandingPageAuditEntryDto>> Handle(GetLandingPageAuditQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.LandingPageAuditEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(entry => entry.EntityType == request.EntityType);
        }

        if (request.EntityId is not null)
        {
            query = query.Where(entry => entry.EntityId == request.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(entry => entry.Action == request.Action);
        }

        var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 200);
        return await query
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Take(limit)
            .Select(entry => new LandingPageAuditEntryDto
            {
                Id = entry.Id,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                Title = entry.Title,
                Description = entry.Description,
                CreatedByUserId = entry.CreatedByUserId,
                OccurredAtUtc = entry.OccurredAtUtc,
            })
            .ToArrayAsync(cancellationToken);
    }
}
