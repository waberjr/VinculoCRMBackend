using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
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

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        var outcome = string.IsNullOrWhiteSpace(request.Outcome) ? (ContactOutcome?)null : SystemOptionMapper.Parse<ContactOutcome>(request.Outcome);
        task.Complete(outcome, request.CompletionNote, DateTimeOffset.UtcNow);

        if (outcome == ContactOutcome.DoNotContact)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(entity => entity.Id == task.DonorId, cancellationToken);
            if (donor is not null)
            {
                donor.DoNotContact = true;
                donor.AllowsCommunication = false;
                donor.DoNotContactReason = request.DoNotContactReason?.Trim();
                donor.Status = DonorStatus.DoNotContact;

                _context.DonorTimelineEntries.Add(new DonorTimelineEntry
                {
                    OrganizationId = task.OrganizationId,
                    DonorId = task.DonorId,
                    Type = TimelineEntryType.Contact,
                    Title = "Contato bloqueado",
                    Description = donor.DoNotContactReason,
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    CreatedByUserId = _user.Id,
                    RelatedEntityType = nameof(RelationshipTask),
                    RelatedEntityId = task.Id,
                });
            }
        }

        if (outcome == ContactOutcome.RequestedCallback && request.FollowUpAtUtc is not null)
        {
            _context.RelationshipTasks.Add(new RelationshipTask
            {
                OrganizationId = task.OrganizationId,
                DonorId = task.DonorId,
                CampaignId = task.CampaignId,
                AssignedUserId = task.AssignedUserId,
                CreatedByUserId = task.CreatedByUserId,
                Type = TaskType.FollowUp,
                Priority = TaskPriority.High,
                Status = RelationshipTaskStatus.Open,
                DueAtUtc = request.FollowUpAtUtc,
                Title = "Retorno solicitado",
                Description = "Follow-up criado automaticamente ao concluir tarefa.",
            });
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = task.OrganizationId,
            DonorId = task.DonorId,
            Type = TimelineEntryType.Contact,
            Title = "Tarefa concluida",
            Description = request.CompletionNote?.Trim(),
            OccurredAtUtc = task.CompletedAtUtc!.Value,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(RelationshipTask),
            RelatedEntityId = task.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
