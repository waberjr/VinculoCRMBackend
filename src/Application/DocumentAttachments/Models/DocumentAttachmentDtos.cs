namespace VinculoBackend.Application.DocumentAttachments.Models;

public sealed record DocumentAttachmentDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Title,
    string Url,
    string? Description,
    DateTimeOffset Created,
    string Kind,
    string? OriginalFileName,
    string? ContentType,
    long? SizeBytes);

public sealed class DocumentAttachmentAuditEntryDto
{
    public Guid Id { get; init; }
    public Guid DocumentAttachmentId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? CreatedByUserId { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
}

public sealed record DocumentAttachmentDownloadDto(string FileName, string ContentType, Stream Content);

public sealed record DocumentAttachmentAccessUrlDto(string Url, DateTimeOffset ExpiresAtUtc);
