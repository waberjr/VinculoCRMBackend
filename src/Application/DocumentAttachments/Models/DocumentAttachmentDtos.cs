namespace VinculoBackend.Application.DocumentAttachments.Models;

public sealed record DocumentAttachmentDto(Guid Id, string EntityType, Guid EntityId, string Title, string Url, string? Description, DateTimeOffset Created);

public sealed record DocumentAttachmentDownloadDto(string FileName, string ContentType, Stream Content);
