using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlerts;

public sealed record GetOperationalAlertsQuery : IRequest<PaginatedResult<OperationalAlertDto>>
{
    public string? Search { get; init; }
    public string? Severity { get; init; }
    public string? Status { get; init; }
    public string? Source { get; init; }
    public string? AssignedUserId { get; init; }
    public bool? OverdueOnly { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetOperationalAlertsQueryHandler : IRequestHandler<GetOperationalAlertsQuery, PaginatedResult<OperationalAlertDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public GetOperationalAlertsQueryHandler(IApplicationDbContext context, IIdentityService identityService, IOrganizationContext organizationContext, ISender sender, TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
        _sender = sender;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedResult<OperationalAlertDto>> Handle(GetOperationalAlertsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        await _sender.Send(new SyncOperationalAlertsCommand(), cancellationToken);
        var query = _context.OperationalAlerts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(alert =>
                alert.Title.Contains(search) ||
                (alert.Description != null && alert.Description.Contains(search)));
        }

        if (TryParse<OperationalAlertSeverity>(request.Severity, out var severity))
        {
            query = query.Where(alert => alert.Severity == severity);
        }

        if (TryParse<OperationalAlertStatus>(request.Status, out var status))
        {
            query = query.Where(alert => alert.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim();
            query = query.Where(alert => alert.Source == source);
        }

        if (!string.IsNullOrWhiteSpace(request.AssignedUserId))
        {
            var assignedUserId = request.AssignedUserId.Trim();
            query = query.Where(alert => alert.AssignedUserId == assignedUserId);
        }

        if (request.OverdueOnly == true)
        {
            var now = _timeProvider.GetUtcNow();
            query = query.Where(alert =>
                alert.Status != OperationalAlertStatus.Resolved &&
                alert.DueAtUtc != null &&
                alert.DueAtUtc < now);
        }

        if (!string.IsNullOrWhiteSpace(request.RelatedEntityType))
        {
            var relatedEntityType = request.RelatedEntityType.Trim();
            query = query.Where(alert => alert.RelatedEntityType == relatedEntityType);
        }

        if (request.RelatedEntityId is not null)
        {
            query = query.Where(alert => alert.RelatedEntityId == request.RelatedEntityId);
        }

        if (request.StartDateUtc is not null)
        {
            query = query.Where(alert => alert.OccurredAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(alert => alert.OccurredAtUtc <= request.EndDateUtc);
        }

        var projected = query
            .OrderBy(alert => alert.Status == OperationalAlertStatus.Resolved)
            .ThenByDescending(alert => alert.Severity)
            .ThenByDescending(alert => alert.OccurredAtUtc)
            .Select(alert => new OperationalAlertDto
            {
                Id = alert.Id,
                Title = alert.Title,
                Description = alert.Description,
                Severity = alert.Severity,
                Status = alert.Status,
                Source = alert.Source,
                RelatedEntityType = alert.RelatedEntityType,
                RelatedEntityId = alert.RelatedEntityId,
                ActionUrl = alert.ActionUrl,
                AssignedUserId = alert.AssignedUserId,
                DueAtUtc = alert.DueAtUtc,
                OccurredAtUtc = alert.OccurredAtUtc,
                AcknowledgedAtUtc = alert.AcknowledgedAtUtc,
                ResolvedAtUtc = alert.ResolvedAtUtc,
                ResolutionNote = alert.ResolutionNote,
                OpenTasksCount = _context.RelationshipTasks.Count(task =>
                    task.OperationalAlertId == alert.Id &&
                    task.Status != RelationshipTaskStatus.Completed &&
                    task.Status != RelationshipTaskStatus.Cancelled),
            });

        var result = await PaginatedResult<OperationalAlertDto>.CreateAsync(
            projected,
            request.PageNumber <= 0 ? 1 : request.PageNumber,
            request.PageSize <= 0 ? 20 : request.PageSize,
            cancellationToken);

        return await WithAssignedUserNames(result);
    }

    private async Task<PaginatedResult<OperationalAlertDto>> WithAssignedUserNames(PaginatedResult<OperationalAlertDto> result)
    {
        var assignedUserIds = result.Items
            .Select(alert => alert.AssignedUserId)
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (assignedUserIds.Length == 0)
        {
            return result;
        }

        var names = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var userId in assignedUserIds)
        {
            names[userId!] = await _identityService.GetUserNameAsync(userId!);
        }

        return new PaginatedResult<OperationalAlertDto>
        {
            Items = result.Items.Select(alert => alert.AssignedUserId is not null && names.TryGetValue(alert.AssignedUserId, out var name)
                ? Copy(alert, name)
                : alert).ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
        };
    }

    private static OperationalAlertDto Copy(OperationalAlertDto alert, string? assignedUserName) => new()
    {
        Id = alert.Id,
        Title = alert.Title,
        Description = alert.Description,
        Severity = alert.Severity,
        Status = alert.Status,
        Source = alert.Source,
        RelatedEntityType = alert.RelatedEntityType,
        RelatedEntityId = alert.RelatedEntityId,
        ActionUrl = alert.ActionUrl,
        AssignedUserId = alert.AssignedUserId,
        AssignedUserName = assignedUserName,
        DueAtUtc = alert.DueAtUtc,
        OccurredAtUtc = alert.OccurredAtUtc,
        AcknowledgedAtUtc = alert.AcknowledgedAtUtc,
        ResolvedAtUtc = alert.ResolvedAtUtc,
        ResolutionNote = alert.ResolutionNote,
        OpenTasksCount = alert.OpenTasksCount,
    };

    private static bool TryParse<TEnum>(string? value, out TEnum result)
        where TEnum : struct
    {
        return Enum.TryParse(value, ignoreCase: true, out result);
    }
}
