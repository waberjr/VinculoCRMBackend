using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivityGoalAudit;

public sealed record GetOperationalProductivityGoalAuditQuery(string? UserId) : IRequest<IReadOnlyCollection<OperationalProductivityGoalAuditEntryDto>>;

public sealed class OperationalProductivityGoalAuditEntryDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public int? PreviousTaskGoalMonthly { get; init; }
    public int? NewTaskGoalMonthly { get; init; }
    public int? PreviousSlaHours { get; init; }
    public int? NewSlaHours { get; init; }
    public string? ChangedByUserId { get; init; }
    public string? ChangedByUserName { get; init; }
    public DateTimeOffset ChangedAtUtc { get; init; }
}

public sealed class GetOperationalProductivityGoalAuditQueryHandler : IRequestHandler<GetOperationalProductivityGoalAuditQuery, IReadOnlyCollection<OperationalProductivityGoalAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;

    public GetOperationalProductivityGoalAuditQueryHandler(IApplicationDbContext context, IIdentityService identityService, IOrganizationContext organizationContext)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OperationalProductivityGoalAuditEntryDto>> Handle(GetOperationalProductivityGoalAuditQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var query = _context.OperationalProductivityGoalAuditEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var userId = request.UserId.Trim();
            query = query.Where(entry => entry.UserId == userId);
        }

        var entries = await query
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Take(50)
            .Select(entry => new OperationalProductivityGoalAuditEntryDto
            {
                Id = entry.Id,
                UserId = entry.UserId,
                PreviousTaskGoalMonthly = entry.PreviousTaskGoalMonthly,
                NewTaskGoalMonthly = entry.NewTaskGoalMonthly,
                PreviousSlaHours = entry.PreviousSlaHours,
                NewSlaHours = entry.NewSlaHours,
                ChangedByUserId = entry.ChangedByUserId,
                ChangedAtUtc = entry.ChangedAtUtc,
            })
            .ToArrayAsync(cancellationToken);

        var result = new List<OperationalProductivityGoalAuditEntryDto>(entries.Length);
        foreach (var entry in entries)
        {
            result.Add(new OperationalProductivityGoalAuditEntryDto
            {
                Id = entry.Id,
                UserId = entry.UserId,
                UserName = await _identityService.GetUserNameAsync(entry.UserId),
                PreviousTaskGoalMonthly = entry.PreviousTaskGoalMonthly,
                NewTaskGoalMonthly = entry.NewTaskGoalMonthly,
                PreviousSlaHours = entry.PreviousSlaHours,
                NewSlaHours = entry.NewSlaHours,
                ChangedByUserId = entry.ChangedByUserId,
                ChangedByUserName = string.IsNullOrWhiteSpace(entry.ChangedByUserId) ? null : await _identityService.GetUserNameAsync(entry.ChangedByUserId),
                ChangedAtUtc = entry.ChangedAtUtc,
            });
        }

        return result;
    }
}
