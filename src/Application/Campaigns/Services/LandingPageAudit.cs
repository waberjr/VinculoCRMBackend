using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Services;

public static class LandingPageAudit
{
    public static LandingPageAuditEntry Create(
        Guid organizationId,
        string entityType,
        Guid entityId,
        string action,
        string title,
        string? description,
        DateTimeOffset occurredAtUtc)
    {
        return new LandingPageAuditEntry
        {
            OrganizationId = organizationId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Title = title,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OccurredAtUtc = occurredAtUtc,
        };
    }
}
