using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.OperationalAlerts.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.AssignOperationalAlert;

public sealed record AssignOperationalAlertCommand(Guid Id, string? AssignedUserId, DateTimeOffset? DueAtUtc) : IRequest;

public sealed class AssignOperationalAlertCommandHandler : IRequestHandler<AssignOperationalAlertCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AssignOperationalAlertCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AssignOperationalAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        var previousAssignedUserId = alert.AssignedUserId;
        var previousDueAtUtc = alert.DueAtUtc;
        alert.Assign(request.AssignedUserId, request.DueAtUtc);
        var now = _timeProvider.GetUtcNow();
        _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
            alert,
            "Assign",
            "Responsavel ou SLA atualizado",
            $"Responsavel: {previousAssignedUserId ?? "-"} -> {alert.AssignedUserId ?? "-"}. Prazo: {previousDueAtUtc?.ToString("O") ?? "-"} -> {alert.DueAtUtc?.ToString("O") ?? "-"}.",
            now,
            _user.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
