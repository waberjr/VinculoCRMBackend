using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

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

    public CompleteRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(CompleteRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks
            .Include(entity => entity.StatusOption)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        if (task.StatusOption.Code is not ("open" or "in-progress"))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(CompleteRelationshipTaskCommand.Id), "Apenas tarefas abertas ou em andamento podem ser concluidas."),
            ]);
        }

        task.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskStatus", "Completed", cancellationToken);
        task.ContactOutcomeOptionId = string.IsNullOrWhiteSpace(request.Outcome)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, "ContactOutcome", request.Outcome, cancellationToken);
        task.CompletedAtUtc = DateTimeOffset.UtcNow;
        task.CompletionNote = request.CompletionNote?.Trim();
        var outcomeCode = string.IsNullOrWhiteSpace(request.Outcome) ? null : ConfigurableOptionCode.FromName(request.Outcome);

        if (outcomeCode == "do-not-contact")
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(entity => entity.Id == task.DonorId, cancellationToken);
            if (donor is not null)
            {
                donor.DoNotContact = true;
                donor.AllowsCommunication = false;
                donor.DoNotContactReason = request.DoNotContactReason?.Trim();
                donor.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonorStatus", "DoNotContact", cancellationToken);

                _context.DonorTimelineEntries.Add(new DonorTimelineEntry
                {
                    OrganizationId = task.OrganizationId,
                    DonorId = task.DonorId,
                    TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", "Contact", cancellationToken),
                    Title = "Contato bloqueado",
                    Description = donor.DoNotContactReason,
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    CreatedByUserId = _user.Id,
                    RelatedEntityType = nameof(RelationshipTask),
                    RelatedEntityId = task.Id,
                });
            }
        }

        if (outcomeCode == "requested-callback" && request.FollowUpAtUtc is not null)
        {
            _context.RelationshipTasks.Add(new RelationshipTask
            {
                OrganizationId = task.OrganizationId,
                DonorId = task.DonorId,
                CampaignId = task.CampaignId,
                AssignedUserId = task.AssignedUserId,
                CreatedByUserId = task.CreatedByUserId,
                TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskType", "FollowUp", cancellationToken),
                PriorityOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskPriority", "High", cancellationToken),
                StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskStatus", "Open", cancellationToken),
                DueAtUtc = request.FollowUpAtUtc,
                Title = "Retorno solicitado",
                Description = "Follow-up criado automaticamente ao concluir tarefa.",
            });
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = task.OrganizationId,
            DonorId = task.DonorId,
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", "Contact", cancellationToken),
            Title = "Tarefa concluida",
            Description = request.CompletionNote?.Trim(),
            OccurredAtUtc = task.CompletedAtUtc.Value,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(RelationshipTask),
            RelatedEntityId = task.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
