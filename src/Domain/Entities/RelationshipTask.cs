using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class RelationshipTask : OrganizationEntity
{
    public Guid DonorId { get; set; }
    public Donor Donor { get; set; } = null!;
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public Guid? DonationId { get; set; }
    public Donation? Donation { get; set; }
    public string? AssignedUserId { get; set; }
    public string? CreatedByUserId { get; set; }
    public TaskType Type { get; set; }
    public TaskPriority Priority { get; set; }
    public RelationshipTaskStatus Status { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public ContactOutcome? ContactOutcome { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CompletionNote { get; set; }

    public void Start()
    {
        if (Status != RelationshipTaskStatus.Open)
        {
            throw new InvalidOperationDomainException("Apenas tarefas abertas podem ser iniciadas.");
        }

        Status = RelationshipTaskStatus.InProgress;
    }

    public void Complete(ContactOutcome? outcome, string? completionNote, DateTimeOffset completedAtUtc)
    {
        if (Status is not (RelationshipTaskStatus.Open or RelationshipTaskStatus.InProgress))
        {
            throw new InvalidOperationDomainException("Apenas tarefas abertas ou em andamento podem ser concluidas.");
        }

        Status = RelationshipTaskStatus.Completed;
        ContactOutcome = outcome;
        CompletedAtUtc = completedAtUtc;
        CompletionNote = completionNote?.Trim();
    }

    public void Cancel()
    {
        if (Status is not (RelationshipTaskStatus.Open or RelationshipTaskStatus.InProgress))
        {
            throw new InvalidOperationDomainException("Apenas tarefas abertas ou em andamento podem ser canceladas.");
        }

        Status = RelationshipTaskStatus.Cancelled;
    }
}
