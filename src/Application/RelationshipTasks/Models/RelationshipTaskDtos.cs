using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.RelationshipTasks.Models;

public sealed class RelationshipTaskListItemDto
{
    public Guid Id { get; init; }
    public Guid DonorId { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public Guid? CampaignId { get; init; }
    public string? CampaignName { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AssignedUserId { get; init; }
    public OptionDto Type { get; init; } = new();
    public OptionDto Priority { get; init; } = new();
    public OptionDto Status { get; init; } = new();
    public DateTimeOffset? DueAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public OptionDto? ContactOutcome { get; init; }
}
