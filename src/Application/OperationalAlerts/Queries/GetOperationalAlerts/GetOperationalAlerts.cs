using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlerts;

public sealed record GetOperationalAlertsQuery : IRequest<PaginatedResult<OperationalAlertDto>>
{
    public string? Search { get; init; }
    public string? Severity { get; init; }
    public string? Status { get; init; }
    public string? Source { get; init; }
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
    private readonly IOrganizationContext _organizationContext;

    public GetOperationalAlertsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<OperationalAlertDto>> Handle(GetOperationalAlertsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
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
                OccurredAtUtc = alert.OccurredAtUtc,
                AcknowledgedAtUtc = alert.AcknowledgedAtUtc,
                ResolvedAtUtc = alert.ResolvedAtUtc,
                ResolutionNote = alert.ResolutionNote,
            });

        return await PaginatedResult<OperationalAlertDto>.CreateAsync(
            projected,
            request.PageNumber <= 0 ? 1 : request.PageNumber,
            request.PageSize <= 0 ? 20 : request.PageSize,
            cancellationToken);
    }

    private static bool TryParse<TEnum>(string? value, out TEnum result)
        where TEnum : struct
    {
        return Enum.TryParse(value, ignoreCase: true, out result);
    }
}
