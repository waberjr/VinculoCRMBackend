using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class RelationshipTask : OrganizationEntity
{
    public Guid? DonorId { get; set; }
    public Donor? Donor { get; set; }
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public Guid? DonationId { get; set; }
    public Donation? Donation { get; set; }
    public Guid? OperationalAlertId { get; set; }
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

    public static RelationshipTask Create(
        Guid organizationId,
        Guid? donorId,
        Guid? campaignId,
        Guid? donationId,
        Guid? operationalAlertId,
        string title,
        string? description,
        string? assignedUserId,
        string? createdByUserId,
        TaskType type,
        TaskPriority priority,
        DateTimeOffset? dueAtUtc)
    {
        var task = new RelationshipTask
        {
            OrganizationId = organizationId,
            Status = RelationshipTaskStatus.Open,
            CreatedByUserId = createdByUserId,
        };
        task.Update(donorId, campaignId, donationId, operationalAlertId, title, description, assignedUserId, type, priority, dueAtUtc);

        return task;
    }

    public void Update(
        Guid? donorId,
        Guid? campaignId,
        Guid? donationId,
        Guid? operationalAlertId,
        string title,
        string? description,
        string? assignedUserId,
        TaskType type,
        TaskPriority priority,
        DateTimeOffset? dueAtUtc)
    {
        EnsureCanBeUpdated();
        SetTitle(title);
        DonorId = donorId;
        CampaignId = campaignId;
        DonationId = donationId;
        OperationalAlertId = operationalAlertId;
        Description = TrimToNull(description);
        AssignedUserId = assignedUserId;
        Type = type;
        Priority = priority;
        DueAtUtc = dueAtUtc;
    }

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

    private void EnsureCanBeUpdated()
    {
        if (Status is RelationshipTaskStatus.Completed or RelationshipTaskStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Tarefas concluidas ou canceladas nao podem ser atualizadas.");
        }
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Informe o titulo da tarefa.");
        }

        Title = title.Trim();
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
