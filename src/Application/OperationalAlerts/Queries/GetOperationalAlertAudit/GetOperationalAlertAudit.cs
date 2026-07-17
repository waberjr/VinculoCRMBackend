using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertAudit;

public sealed record GetOperationalAlertAuditQuery(Guid AlertId) : IRequest<IReadOnlyCollection<OperationalAlertAuditEntryDto>>;

public sealed class GetOperationalAlertAuditQueryHandler : IRequestHandler<GetOperationalAlertAuditQuery, IReadOnlyCollection<OperationalAlertAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetOperationalAlertAuditQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OperationalAlertAuditEntryDto>> Handle(GetOperationalAlertAuditQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        return await _context.OperationalAlertAuditEntries
            .AsNoTracking()
            .Where(entry => entry.OperationalAlertId == request.AlertId)
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Select(entry => new OperationalAlertAuditEntryDto
            {
                Id = entry.Id,
                OperationalAlertId = entry.OperationalAlertId,
                Action = entry.Action,
                Title = entry.Title,
                Description = entry.Description,
                CreatedByUserId = entry.CreatedByUserId,
                OccurredAtUtc = entry.OccurredAtUtc,
            })
            .ToArrayAsync(cancellationToken);
    }
}
