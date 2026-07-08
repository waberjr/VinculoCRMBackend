using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.ImpactUpdates.Models;

namespace VinculoBackend.Application.ImpactUpdates.Queries.GetImpactUpdates;

public sealed record GetImpactUpdatesQuery(Guid? ProjectId) : IRequest<IReadOnlyCollection<ImpactUpdateDto>>;

public sealed class GetImpactUpdatesQueryHandler : IRequestHandler<GetImpactUpdatesQuery, IReadOnlyCollection<ImpactUpdateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetImpactUpdatesQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<ImpactUpdateDto>> Handle(GetImpactUpdatesQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var query = _context.ImpactUpdates.AsNoTracking();
        if (request.ProjectId is not null)
        {
            query = query.Where(update => update.ProjectId == request.ProjectId);
        }

        return await query
            .OrderByDescending(update => update.PublishedAtUtc)
            .Take(50)
            .Select(update => new ImpactUpdateDto(update.Id, update.ProjectId, update.Title, update.Content, update.PublishedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
