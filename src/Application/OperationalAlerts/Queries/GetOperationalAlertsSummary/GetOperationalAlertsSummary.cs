using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.SyncOperationalAlerts;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertsSummary;

public sealed record GetOperationalAlertsSummaryQuery : IRequest<OperationalAlertsSummaryDto>;

public sealed record OperationalAlertsSummaryDto(int OpenCount, int HighCount, int OverdueCount);

public sealed class GetOperationalAlertsSummaryQueryHandler : IRequestHandler<GetOperationalAlertsSummaryQuery, OperationalAlertsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public GetOperationalAlertsSummaryQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext, ISender sender, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _sender = sender;
        _timeProvider = timeProvider;
    }

    public async Task<OperationalAlertsSummaryDto> Handle(GetOperationalAlertsSummaryQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        await _sender.Send(new SyncOperationalAlertsCommand(), cancellationToken);
        var now = _timeProvider.GetUtcNow();

        var openCount = await _context.OperationalAlerts.AsNoTracking().CountAsync(alert => alert.Status != OperationalAlertStatus.Resolved, cancellationToken);
        var highCount = await _context.OperationalAlerts.AsNoTracking().CountAsync(alert =>
            alert.Status != OperationalAlertStatus.Resolved &&
            (alert.Severity == OperationalAlertSeverity.High || alert.Severity == OperationalAlertSeverity.Critical), cancellationToken);
        var overdueCount = await _context.OperationalAlerts.AsNoTracking().CountAsync(alert =>
            alert.Status != OperationalAlertStatus.Resolved &&
            alert.DueAtUtc != null &&
            alert.DueAtUtc < now, cancellationToken);

        return new OperationalAlertsSummaryDto(openCount, highCount, overdueCount);
    }
}
