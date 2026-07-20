using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertAudit;

public sealed record GetOperationalAlertAuditQuery(Guid AlertId) : IRequest<IReadOnlyCollection<OperationalAlertAuditEntryDto>>;

public sealed class GetOperationalAlertAuditQueryHandler : IRequestHandler<GetOperationalAlertAuditQuery, IReadOnlyCollection<OperationalAlertAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;

    public GetOperationalAlertAuditQueryHandler(IApplicationDbContext context, IIdentityService identityService, IOrganizationContext organizationContext)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OperationalAlertAuditEntryDto>> Handle(GetOperationalAlertAuditQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var entries = await _context.OperationalAlertAuditEntries
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

        var userIds = entries
            .Select(entry => entry.CreatedByUserId)
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (userIds.Length == 0)
        {
            return entries;
        }

        var names = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var userId in userIds)
        {
            names[userId!] = await _identityService.GetUserNameAsync(userId!);
        }

        return entries.Select(entry => entry.CreatedByUserId is not null && names.TryGetValue(entry.CreatedByUserId, out var name)
            ? Copy(entry, name)
            : entry).ToArray();
    }

    private static OperationalAlertAuditEntryDto Copy(OperationalAlertAuditEntryDto entry, string? createdByUserName) => new()
    {
        Id = entry.Id,
        OperationalAlertId = entry.OperationalAlertId,
        Action = entry.Action,
        Title = entry.Title,
        Description = entry.Description,
        CreatedByUserId = entry.CreatedByUserId,
        CreatedByUserName = createdByUserName,
        OccurredAtUtc = entry.OccurredAtUtc,
    };
}
