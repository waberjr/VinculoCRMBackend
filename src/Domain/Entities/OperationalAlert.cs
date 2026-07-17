using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class OperationalAlert : OrganizationEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public OperationalAlertSeverity Severity { get; private set; }
    public OperationalAlertStatus Status { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? AssignedUserId { get; private set; }
    public DateTimeOffset? DueAtUtc { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? AcknowledgedAtUtc { get; private set; }
    public string? AcknowledgedByUserId { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public string? ResolvedByUserId { get; private set; }
    public string? ResolutionNote { get; private set; }

    public static OperationalAlert Create(
        Guid organizationId,
        string title,
        string? description,
        OperationalAlertSeverity severity,
        string source,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? actionUrl,
        string? assignedUserId,
        DateTimeOffset? dueAtUtc,
        DateTimeOffset occurredAtUtc)
    {
        var alert = new OperationalAlert
        {
            OrganizationId = organizationId,
            Severity = severity,
            Status = OperationalAlertStatus.Open,
            OccurredAtUtc = occurredAtUtc,
        };
        alert.SetTitle(title);
        alert.Description = TrimToNull(description);
        alert.Source = Required(source, "Informe a origem do alerta.");
        alert.RelatedEntityType = TrimToNull(relatedEntityType);
        alert.RelatedEntityId = relatedEntityId;
        alert.ActionUrl = TrimToNull(actionUrl);
        alert.AssignedUserId = TrimToNull(assignedUserId);
        alert.DueAtUtc = dueAtUtc;
        return alert;
    }

    public void Refresh(string? description, OperationalAlertSeverity severity, string? assignedUserId, DateTimeOffset? dueAtUtc, DateTimeOffset occurredAtUtc)
    {
        if (Status == OperationalAlertStatus.Resolved)
        {
            Status = OperationalAlertStatus.Open;
            ResolvedAtUtc = null;
            ResolvedByUserId = null;
            ResolutionNote = null;
        }

        Description = TrimToNull(description);
        Severity = severity;
        AssignedUserId = TrimToNull(assignedUserId);
        DueAtUtc = dueAtUtc;
        OccurredAtUtc = occurredAtUtc;
    }

    public void Acknowledge(string? userId, DateTimeOffset acknowledgedAtUtc)
    {
        if (Status == OperationalAlertStatus.Resolved)
        {
            throw new InvalidOperationDomainException("Alertas resolvidos nao podem ser assumidos.");
        }

        Status = OperationalAlertStatus.Acknowledged;
        AcknowledgedAtUtc = acknowledgedAtUtc;
        AcknowledgedByUserId = TrimToNull(userId);
    }

    public void Assign(string? userId, DateTimeOffset? dueAtUtc)
    {
        if (Status == OperationalAlertStatus.Resolved)
        {
            throw new InvalidOperationDomainException("Alertas resolvidos nao podem ser atribuidos.");
        }

        AssignedUserId = TrimToNull(userId);
        DueAtUtc = dueAtUtc;
    }

    public void Resolve(string? userId, string? note, DateTimeOffset resolvedAtUtc)
    {
        if (Status == OperationalAlertStatus.Resolved)
        {
            throw new InvalidOperationDomainException("Alerta ja resolvido.");
        }

        Status = OperationalAlertStatus.Resolved;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByUserId = TrimToNull(userId);
        ResolutionNote = TrimToNull(note);
    }

    private void SetTitle(string title)
    {
        Title = Required(title, "Informe o titulo do alerta.");
    }

    private static string Required(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(message);
        }

        return value.Trim();
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
