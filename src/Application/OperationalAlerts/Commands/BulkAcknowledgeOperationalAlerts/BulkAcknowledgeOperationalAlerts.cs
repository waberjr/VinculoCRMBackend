using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.OperationalAlerts.Services;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.OperationalAlerts.Commands.BulkAcknowledgeOperationalAlerts;

public sealed record BulkAcknowledgeOperationalAlertsCommand(IReadOnlyCollection<Guid> AlertIds) : IRequest<int>;

public sealed class BulkAcknowledgeOperationalAlertsCommandHandler : IRequestHandler<BulkAcknowledgeOperationalAlertsCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public BulkAcknowledgeOperationalAlertsCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<int> Handle(BulkAcknowledgeOperationalAlertsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.AlertIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return 0;
        }

        var alerts = await _context.OperationalAlerts
            .Where(alert => ids.Contains(alert.Id) && alert.Status != OperationalAlertStatus.Resolved)
            .ToArrayAsync(cancellationToken);
        var now = _timeProvider.GetUtcNow();
        foreach (var alert in alerts)
        {
            alert.Acknowledge(_user.Id, now);
            _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(alert, "BulkAcknowledge", "Alerta assumido em lote", null, now, _user.Id));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return alerts.Length;
    }
}
