using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.OperationalAlerts.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.AcknowledgeOperationalAlert;

public sealed record AcknowledgeOperationalAlertCommand(Guid Id) : IRequest;

public sealed class AcknowledgeOperationalAlertCommandHandler : IRequestHandler<AcknowledgeOperationalAlertCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AcknowledgeOperationalAlertCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AcknowledgeOperationalAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        var now = _timeProvider.GetUtcNow();
        alert.Acknowledge(_user.Id, now);
        _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(alert, "Acknowledge", "Alerta assumido", null, now, _user.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
