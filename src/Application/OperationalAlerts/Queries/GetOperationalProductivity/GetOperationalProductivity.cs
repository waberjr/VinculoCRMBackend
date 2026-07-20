using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivity;

public sealed record GetOperationalProductivityQuery(
    string? AssignedUserId,
    string? Source,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc) : IRequest<OperationalProductivityDto>;

public sealed record OperationalProductivityDto(
    DateTimeOffset StartDateUtc,
    DateTimeOffset EndDateUtc,
    int CreatedTasksCount,
    int CompletedTasksCount,
    int OverdueCompletedTasksCount,
    int ResolvedAlertsCount,
    IReadOnlyCollection<OperationalProductivityItemDto> Items);

public sealed record OperationalProductivityItemDto(
    string? AssignedUserId,
    string AssignedUserName,
    int CreatedTasksCount,
    int CompletedTasksCount,
    int OverdueCompletedTasksCount,
    int ResolvedAlertsCount);

public sealed class GetOperationalProductivityQueryHandler : IRequestHandler<GetOperationalProductivityQuery, OperationalProductivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetOperationalProductivityQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<OperationalProductivityDto> Handle(GetOperationalProductivityQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var now = _timeProvider.GetUtcNow();
        var start = request.StartDateUtc ?? new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = request.EndDateUtc ?? now;

        var tasks = _context.RelationshipTasks.AsNoTracking()
            .Where(task => task.DonorId == null && task.OperationalAlertId != null);
        var alerts = _context.OperationalAlerts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.AssignedUserId))
        {
            var assignedUserId = request.AssignedUserId.Trim();
            tasks = tasks.Where(task => task.AssignedUserId == assignedUserId);
            alerts = alerts.Where(alert => alert.AssignedUserId == assignedUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim();
            tasks = tasks.Where(task =>
                _context.OperationalAlerts.Any(alert => alert.Id == task.OperationalAlertId && alert.Source == source));
            alerts = alerts.Where(alert => alert.Source == source);
        }

        var createdTaskGroups = await tasks
            .Where(task => task.Created >= start && task.Created <= end)
            .GroupBy(task => task.AssignedUserId)
            .Select(group => new { AssignedUserId = group.Key, Count = group.Count() })
            .ToArrayAsync(cancellationToken);
        var completedTaskGroups = await tasks
            .Where(task =>
                task.Status == RelationshipTaskStatus.Completed &&
                task.CompletedAtUtc != null &&
                task.CompletedAtUtc >= start &&
                task.CompletedAtUtc <= end)
            .GroupBy(task => task.AssignedUserId)
            .Select(group => new
            {
                AssignedUserId = group.Key,
                Count = group.Count(),
                OverdueCount = group.Count(task => task.DueAtUtc != null && task.CompletedAtUtc > task.DueAtUtc),
            })
            .ToArrayAsync(cancellationToken);
        var resolvedAlertGroups = await alerts
            .Where(alert =>
                alert.Status == OperationalAlertStatus.Resolved &&
                alert.ResolvedAtUtc != null &&
                alert.ResolvedAtUtc >= start &&
                alert.ResolvedAtUtc <= end)
            .GroupBy(alert => alert.AssignedUserId)
            .Select(group => new { AssignedUserId = group.Key, Count = group.Count() })
            .ToArrayAsync(cancellationToken);

        var userIds = createdTaskGroups.Select(group => group.AssignedUserId)
            .Concat(completedTaskGroups.Select(group => group.AssignedUserId))
            .Concat(resolvedAlertGroups.Select(group => group.AssignedUserId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var items = new List<OperationalProductivityItemDto>();
        foreach (var userId in userIds)
        {
            var createdTasks = createdTaskGroups.FirstOrDefault(group => group.AssignedUserId == userId)?.Count ?? 0;
            var completedTaskGroup = completedTaskGroups.FirstOrDefault(group => group.AssignedUserId == userId);
            var resolvedAlerts = resolvedAlertGroups.FirstOrDefault(group => group.AssignedUserId == userId)?.Count ?? 0;
            var name = string.IsNullOrWhiteSpace(userId)
                ? "Sem responsavel"
                : await _identityService.GetUserNameAsync(userId);

            items.Add(new OperationalProductivityItemDto(
                userId,
                string.IsNullOrWhiteSpace(name) ? userId ?? "Sem responsavel" : name,
                createdTasks,
                completedTaskGroup?.Count ?? 0,
                completedTaskGroup?.OverdueCount ?? 0,
                resolvedAlerts));
        }

        var orderedItems = items
            .OrderByDescending(item => item.CompletedTasksCount + item.ResolvedAlertsCount)
            .ThenBy(item => item.AssignedUserName)
            .ToArray();

        return new OperationalProductivityDto(
            start,
            end,
            orderedItems.Sum(item => item.CreatedTasksCount),
            orderedItems.Sum(item => item.CompletedTasksCount),
            orderedItems.Sum(item => item.OverdueCompletedTasksCount),
            orderedItems.Sum(item => item.ResolvedAlertsCount),
            orderedItems);
    }
}
