namespace VinculoBackend.Application.ImpactUpdates.Models;

public sealed record ImpactUpdateDto(Guid Id, Guid ProjectId, string Title, string Content, DateTimeOffset PublishedAtUtc);
