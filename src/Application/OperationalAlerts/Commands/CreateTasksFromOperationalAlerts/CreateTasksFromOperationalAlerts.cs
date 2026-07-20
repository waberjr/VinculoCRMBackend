using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Services;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Application.OperationalAlerts.Commands.CreateTasksFromOperationalAlerts;

public sealed record CreateTasksFromOperationalAlertsCommand(
    IReadOnlyCollection<Guid> AlertIds,
    string Title,
    string? Description,
    string Type,
    string Priority,
    DateTimeOffset? DueAtUtc,
    string? AssignedUserId) : IRequest<int>;

public sealed class CreateTasksFromOperationalAlertsCommandHandler : IRequestHandler<CreateTasksFromOperationalAlertsCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CreateTasksFromOperationalAlertsCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<int> Handle(CreateTasksFromOperationalAlertsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainValidationException("Informe o titulo das tarefas.");
        }

        var ids = request.AlertIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return 0;
        }

        var alerts = await _context.OperationalAlerts
            .Where(alert => ids.Contains(alert.Id) && alert.Status != OperationalAlertStatus.Resolved)
            .ToArrayAsync(cancellationToken);
        var now = _timeProvider.GetUtcNow();
        var created = 0;
        foreach (var alert in alerts)
        {
            var title = request.Title.Trim();
            var description = string.IsNullOrWhiteSpace(request.Description)
                ? $"Tarefa criada em lote a partir do alerta: {alert.Title}"
                : request.Description.Trim();
            var task = RelationshipTask.Create(
                alert.OrganizationId,
                null,
                null,
                null,
                alert.Id,
                title,
                description,
                request.AssignedUserId,
                _user.Id,
                SystemOptionMapper.Parse<TaskType>(request.Type),
                SystemOptionMapper.Parse<TaskPriority>(request.Priority),
                request.DueAtUtc);
            _context.RelationshipTasks.Add(task);
            _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
                alert,
                "TaskCreated",
                "Tarefa criada em lote",
                $"Tarefa {task.Id}: {task.Title}",
                now,
                _user.Id));
            created++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return created;
    }
}
