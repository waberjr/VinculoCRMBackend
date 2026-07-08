using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Infrastructure.Storage;

public sealed class AzureBlobFileStorageService : IFileStorageService
{
    private const string Provider = "azure-blob";
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobFileStorageService> _logger;

    public AzureBlobFileStorageService(IOptions<AzureBlobFileStorageOptions> options, ILogger<AzureBlobFileStorageService> logger)
    {
        var storageOptions = options.Value;
        _containerClient = new BlobContainerClient(storageOptions.ConnectionString, storageOptions.ContainerName);
        _logger = logger;
    }

    public async Task<StoredFileReference> StoreAsync(StoreFileRequest request, CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = string.Join(
            '/',
            request.OrganizationId.ToString("N"),
            SanitizeSegment(request.EntityType),
            request.EntityId.ToString("N"),
            $"{Guid.NewGuid():N}-{SanitizeFileName(request.FileName)}");

        var blob = _containerClient.GetBlobClient(blobName);
        await blob.UploadAsync(
            request.Content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(request.ContentType)
                        ? "application/octet-stream"
                        : request.ContentType,
                },
                Metadata = new Dictionary<string, string>
                {
                    ["organizationId"] = request.OrganizationId.ToString("N"),
                    ["entityType"] = request.EntityType,
                    ["entityId"] = request.EntityId.ToString("N"),
                    ["originalFileName"] = request.FileName,
                },
            },
            cancellationToken);

        _logger.LogInformation(
            "Document file stored in blob storage for organization {OrganizationId}, entity {EntityType}/{EntityId}.",
            request.OrganizationId,
            SanitizeSegment(request.EntityType),
            request.EntityId);

        return new StoredFileReference(Provider, blobName, $"storage://{Provider}/{blobName}");
    }

    public async Task<StoredFileDownload?> OpenReadAsync(string internalUri, CancellationToken cancellationToken)
    {
        if (!TryGetBlobName(internalUri, out var blobName))
        {
            return null;
        }

        var blob = _containerClient.GetBlobClient(blobName);
        if (!await blob.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);
        var content = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        var fileName = blobName.Split('/').Last();
        var originalFileName = properties.Value.Metadata.TryGetValue("originalFileName", out var value)
            ? value
            : fileName;

        return new StoredFileDownload(
            originalFileName,
            properties.Value.ContentType ?? "application/octet-stream",
            content);
    }

    public async Task DeleteAsync(string internalUri, CancellationToken cancellationToken)
    {
        if (!TryGetBlobName(internalUri, out var blobName))
        {
            return;
        }

        var deleted = await _containerClient.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
        if (deleted.Value)
        {
            _logger.LogInformation("Document file removed from blob storage.");
        }
    }

    public async Task<TemporaryFileAccessUrl?> CreateTemporaryReadUrlAsync(string internalUri, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (!TryGetBlobName(internalUri, out var blobName))
        {
            return null;
        }

        var blob = _containerClient.GetBlobClient(blobName);
        if (!blob.CanGenerateSasUri || !await blob.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.Add(ttl);
        var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, expiresAtUtc);
        return new TemporaryFileAccessUrl(sasUri.ToString(), expiresAtUtc);
    }

    private static bool TryGetBlobName(string internalUri, out string blobName)
    {
        const string prefix = $"storage://{Provider}/";
        if (!internalUri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            blobName = string.Empty;
            return false;
        }

        blobName = internalUri[prefix.Length..];
        return !string.IsNullOrWhiteSpace(blobName);
    }

    private static string SanitizeSegment(string value)
    {
        var sanitized = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "entity" : sanitized;
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "arquivo" : sanitized;
    }
}
