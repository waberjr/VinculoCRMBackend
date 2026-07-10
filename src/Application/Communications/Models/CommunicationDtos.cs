namespace VinculoBackend.Application.Communications.Models;

public sealed class CommunicationTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string? Subject { get; init; }
    public string Body { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Variables { get; init; } = [];
    public bool IsActive { get; init; }
    public DateTimeOffset Created { get; init; }
}

public sealed class CommunicationCampaignDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public Guid? TemplateId { get; init; }
    public string? TemplateName { get; init; }
    public DateTimeOffset? ScheduledAtUtc { get; init; }
    public DateTimeOffset PlannedAtUtc { get; init; }
    public int RecipientsCount { get; init; }
    public int BlockedByConsentCount { get; init; }
}
