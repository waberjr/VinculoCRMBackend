using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalWorkload;

public sealed record GetOperationalWorkloadQuery(
    string? AssignedUserId,
    string? Source,
    bool? OverdueOnly) : IRequest<IReadOnlyCollection<OperationalWorkloadItemDto>>;

public sealed record OperationalWorkloadItemDto(
    string? AssignedUserId,
    string AssignedUserName,
    int AlertsCount,
    int HighAlertsCount,
    int OpenTasksCount,
    int OverdueTasksCount);

public sealed class GetOperationalWorkloadQueryHandler : IRequestHandler<GetOperationalWorkloadQuery, IReadOnlyCollection<OperationalWorkloadItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public GetOperationalWorkloadQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IOrganizationContext organizationContext,
        ISender sender,
        TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
        _sender = sender;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<OperationalWorkloadItemDto>> Handle(GetOperationalWorkloadQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        await _sender.Send(new SyncOperationalAlertsCommand(), cancellationToken);

        var now = _timeProvider.GetUtcNow();
        var alerts = _context.OperationalAlerts.AsNoTracking()
            .Where(alert => alert.Status != OperationalAlertStatus.Resolved);
        var tasks = _context.RelationshipTasks.AsNoTracking()
            .Where(task =>
                task.DonorId == null &&
                task.OperationalAlertId != null &&
                (task.Status == RelationshipTaskStatus.Open || task.Status == RelationshipTaskStatus.InProgress));

        if (!string.IsNullOrWhiteSpace(request.AssignedUserId))
        {
            var assignedUserId = request.AssignedUserId.Trim();
            alerts = alerts.Where(alert => alert.AssignedUserId == assignedUserId);
            tasks = tasks.Where(task => task.AssignedUserId == assignedUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim();
            alerts = alerts.Where(alert => alert.Source == source);
            tasks = tasks.Where(task =>
                _context.OperationalAlerts.Any(alert => alert.Id == task.OperationalAlertId && alert.Source == source));
        }

        if (request.OverdueOnly == true)
        {
            alerts = alerts.Where(alert => alert.DueAtUtc != null && alert.DueAtUtc < now);
            tasks = tasks.Where(task => task.DueAtUtc != null && task.DueAtUtc < now);
        }

        var alertGroups = await alerts
            .GroupBy(alert => alert.AssignedUserId)
            .Select(group => new
            {
                AssignedUserId = group.Key,
                AlertsCount = group.Count(),
                HighAlertsCount = group.Count(alert =>
                    alert.Severity == OperationalAlertSeverity.High ||
                    alert.Severity == OperationalAlertSeverity.Critical),
            })
            .ToArrayAsync(cancellationToken);
        var taskGroups = await tasks
            .GroupBy(task => task.AssignedUserId)
            .Select(group => new
            {
                AssignedUserId = group.Key,
                OpenTasksCount = group.Count(),
                OverdueTasksCount = group.Count(task => task.DueAtUtc != null && task.DueAtUtc < now),
            })
            .ToArrayAsync(cancellationToken);

        var userIds = alertGroups.Select(group => group.AssignedUserId)
            .Concat(taskGroups.Select(group => group.AssignedUserId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var items = new List<OperationalWorkloadItemDto>();
        foreach (var userId in userIds)
        {
            var alertGroup = alertGroups.FirstOrDefault(group => group.AssignedUserId == userId);
            var taskGroup = taskGroups.FirstOrDefault(group => group.AssignedUserId == userId);
            var name = string.IsNullOrWhiteSpace(userId)
                ? "Sem responsavel"
                : await _identityService.GetUserNameAsync(userId);

            items.Add(new OperationalWorkloadItemDto(
                userId,
                string.IsNullOrWhiteSpace(name) ? userId ?? "Sem responsavel" : name,
                alertGroup?.AlertsCount ?? 0,
                alertGroup?.HighAlertsCount ?? 0,
                taskGroup?.OpenTasksCount ?? 0,
                taskGroup?.OverdueTasksCount ?? 0));
        }

        return items
            .OrderByDescending(item => item.OverdueTasksCount)
            .ThenByDescending(item => item.HighAlertsCount)
            .ThenByDescending(item => item.AlertsCount + item.OpenTasksCount)
            .ToArray();
    }
}
