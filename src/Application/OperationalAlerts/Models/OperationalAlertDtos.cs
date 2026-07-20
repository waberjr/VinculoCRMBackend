using VinculoBackend.Domain.Enums;
using VinculoBackend.Application.RelationshipTasks.Models;

namespace VinculoBackend.Application.OperationalAlerts.Models;

public sealed class OperationalAlertDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public OperationalAlertSeverity Severity { get; init; }
    public OperationalAlertStatus Status { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? ActionUrl { get; init; }
    public string? AssignedUserId { get; init; }
    public string? AssignedUserName { get; init; }
    public DateTimeOffset? DueAtUtc { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public DateTimeOffset? AcknowledgedAtUtc { get; init; }
    public DateTimeOffset? ResolvedAtUtc { get; init; }
    public string? ResolutionNote { get; init; }
    public int OpenTasksCount { get; init; }
    public int InProgressTasksCount { get; init; }
    public int CompletedTasksCount { get; init; }
    public string? LastCompletedTaskTitle { get; init; }
    public DateTimeOffset? LastCompletedTaskCompletedAtUtc { get; init; }
}

public sealed class OperationalAlertDetailDto
{
    public OperationalAlertDto Alert { get; init; } = new();
    public IReadOnlyCollection<OperationalAlertAuditEntryDto> AuditEntries { get; init; } = [];
    public IReadOnlyCollection<RelationshipTaskListItemDto> Tasks { get; init; } = [];
}

public sealed class OperationalAlertAuditEntryDto
{
    public Guid Id { get; init; }
    public Guid OperationalAlertId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CreatedByUserId { get; init; }
    public string? CreatedByUserName { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
}
