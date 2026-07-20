using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;
using VinculoBackend.Application.OperationalAlerts.Services;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CompleteRelationshipTask;

public record CompleteRelationshipTaskCommand(
    Guid Id,
    string? Outcome,
    string? CompletionNote,
    DateTimeOffset? FollowUpAtUtc,
    string? DoNotContactReason) : IRequest;

public sealed class CompleteRelationshipTaskCommandHandler : IRequestHandler<CompleteRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CompleteRelationshipTaskCommandHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(CompleteRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        var now = _timeProvider.GetUtcNow();
        var outcome = string.IsNullOrWhiteSpace(request.Outcome) ? (ContactOutcome?)null : SystemOptionMapper.Parse<ContactOutcome>(request.Outcome);
        task.Complete(outcome, request.CompletionNote, now);

        if (outcome == ContactOutcome.DoNotContact && task.DonorId is not null)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(entity => entity.Id == task.DonorId, cancellationToken);
            if (donor is not null)
            {
                donor.BlockContact(request.DoNotContactReason);

                _context.DonorTimelineEntries.Add(new DonorTimelineEntry
                {
                    OrganizationId = task.OrganizationId,
                    DonorId = task.DonorId.Value,
                    Type = TimelineEntryType.Contact,
                    Title = "Contato bloqueado",
                    Description = donor.DoNotContactReason,
                    OccurredAtUtc = now,
                    CreatedByUserId = _user.Id,
                    RelatedEntityType = nameof(RelationshipTask),
                    RelatedEntityId = task.Id,
                });
            }
        }

        if (outcome == ContactOutcome.RequestedCallback && request.FollowUpAtUtc is not null && task.DonorId is not null)
        {
            _context.RelationshipTasks.Add(RelationshipTask.Create(
                task.OrganizationId,
                task.DonorId,
                task.CampaignId,
                null,
                task.OperationalAlertId,
                "Retorno solicitado",
                "Follow-up criado automaticamente ao concluir tarefa.",
                task.AssignedUserId,
                task.CreatedByUserId,
                TaskType.FollowUp,
                TaskPriority.High,
                request.FollowUpAtUtc));
        }

        if (task.DonorId is not null)
        {
            _context.DonorTimelineEntries.Add(new DonorTimelineEntry
            {
                OrganizationId = task.OrganizationId,
                DonorId = task.DonorId.Value,
                Type = TimelineEntryType.Contact,
                Title = "Tarefa concluida",
                Description = request.CompletionNote?.Trim(),
                OccurredAtUtc = task.CompletedAtUtc!.Value,
                CreatedByUserId = _user.Id,
                RelatedEntityType = nameof(RelationshipTask),
                RelatedEntityId = task.Id,
            });
        }

        if (task.OperationalAlertId is not null)
        {
            var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == task.OperationalAlertId, cancellationToken);
            if (alert is not null)
            {
                _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
                    alert,
                    "TaskCompleted",
                    "Tarefa vinculada concluida",
                    $"{task.Title}: {request.CompletionNote?.Trim() ?? "Sem nota."}",
                    now,
                    _user.Id));

                var hasIncompleteLinkedTasks = await _context.RelationshipTasks.AsNoTracking().AnyAsync(entity =>
                    entity.OperationalAlertId == task.OperationalAlertId &&
                    entity.Id != task.Id &&
                    entity.Status != RelationshipTaskStatus.Completed,
                    cancellationToken);
                if (!hasIncompleteLinkedTasks && alert.Status != OperationalAlertStatus.Resolved)
                {
                    const string resolutionNote = "Resolvido automaticamente porque todas as tarefas vinculadas foram concluidas.";
                    alert.Resolve(_user.Id, resolutionNote, now);
                    _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
                        alert,
                        "AutoResolve",
                        "Alerta resolvido automaticamente",
                        resolutionNote,
                        now,
                        _user.Id));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
