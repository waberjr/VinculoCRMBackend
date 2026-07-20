using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertIds;

public sealed record GetOperationalAlertIdsQuery : IRequest<IReadOnlyCollection<Guid>>
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
}

public sealed class GetOperationalAlertIdsQueryHandler : IRequestHandler<GetOperationalAlertIdsQuery, IReadOnlyCollection<Guid>>
{
    private const int MaxSelectionSize = 1000;

    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public GetOperationalAlertIdsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext, ISender sender, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _sender = sender;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<Guid>> Handle(GetOperationalAlertIdsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        await _sender.Send(new SyncOperationalAlertsCommand(), cancellationToken);

        var query = _context.OperationalAlerts
            .AsNoTracking()
            .Where(alert => alert.Status != OperationalAlertStatus.Resolved);

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

        if (TryParse<OperationalAlertStatus>(request.Status, out var status) && status != OperationalAlertStatus.Resolved)
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

        return await query
            .OrderByDescending(alert => alert.Severity)
            .ThenByDescending(alert => alert.OccurredAtUtc)
            .Select(alert => alert.Id)
            .Take(MaxSelectionSize)
            .ToArrayAsync(cancellationToken);
    }

    private static bool TryParse<TEnum>(string? value, out TEnum result)
        where TEnum : struct
    {
        return Enum.TryParse(value, ignoreCase: true, out result);
    }
}
