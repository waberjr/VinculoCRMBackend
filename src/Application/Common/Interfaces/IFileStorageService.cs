namespace VinculoBackend.Application.Common.Interfaces;

public sealed record StoreFileRequest(
    Guid OrganizationId,
    string EntityType,
    Guid EntityId,
    string FileName,
    string ContentType,
    Stream Content,
    long Length);

public sealed record StoredFileReference(string Provider, string Key, string InternalUri);

public sealed record StoredFileDownload(string FileName, string ContentType, Stream Content);

public sealed record TemporaryFileAccessUrl(string Url, DateTimeOffset ExpiresAtUtc);

public interface IFileStorageService
{
    Task<StoredFileReference> StoreAsync(StoreFileRequest request, CancellationToken cancellationToken);

    Task<StoredFileDownload?> OpenReadAsync(string internalUri, CancellationToken cancellationToken);

    Task DeleteAsync(string internalUri, CancellationToken cancellationToken);

    Task<TemporaryFileAccessUrl?> CreateTemporaryReadUrlAsync(string internalUri, TimeSpan ttl, CancellationToken cancellationToken);
}
