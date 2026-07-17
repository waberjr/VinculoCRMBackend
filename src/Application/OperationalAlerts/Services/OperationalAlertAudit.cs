using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Services;

public static class OperationalAlertAudit
{
    public static OperationalAlertAuditEntry Create(
        OperationalAlert alert,
        string action,
        string title,
        string? description,
        DateTimeOffset occurredAtUtc,
        string? userId)
    {
        return new OperationalAlertAuditEntry
        {
            OrganizationId = alert.OrganizationId,
            OperationalAlertId = alert.Id,
            Action = action,
            Title = title,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OccurredAtUtc = occurredAtUtc,
            CreatedByUserId = userId,
        };
    }
}
