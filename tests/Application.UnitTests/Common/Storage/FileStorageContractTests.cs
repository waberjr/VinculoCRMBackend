using System.Text;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.UnitTests.Common.Storage;

public class FileStorageContractTests
{
    [Test]
    public async Task ShouldStoreAndReadUsingProviderIndependentContract()
    {
        var storage = new InMemoryFileStorageService();
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("conteudo do documento"));

        var stored = await storage.StoreAsync(
            new StoreFileRequest(
                Guid.NewGuid(),
                "Donor",
                Guid.NewGuid(),
                "comprovante.txt",
                "text/plain",
                content,
                content.Length),
            CancellationToken.None);

        stored.InternalUri.ShouldStartWith("storage://memory/");

        var download = await storage.OpenReadAsync(stored.InternalUri, CancellationToken.None);

        download.ShouldNotBeNull();
        download.FileName.ShouldBe("comprovante.txt");
        download.ContentType.ShouldBe("text/plain");
    }

    private sealed class InMemoryFileStorageService : IFileStorageService
    {
        private readonly Dictionary<string, StoredFileDownload> _files = [];

        public async Task<StoredFileReference> StoreAsync(StoreFileRequest request, CancellationToken cancellationToken)
        {
            var key = Guid.NewGuid().ToString("N");
            var buffer = new MemoryStream();
            await request.Content.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;
            _files[key] = new StoredFileDownload(request.FileName, request.ContentType, buffer);
            return new StoredFileReference("memory", key, $"storage://memory/{key}");
        }

        public Task<StoredFileDownload?> OpenReadAsync(string internalUri, CancellationToken cancellationToken)
        {
            var key = internalUri["storage://memory/".Length..];
            return Task.FromResult(_files.GetValueOrDefault(key));
        }

        public Task DeleteAsync(string internalUri, CancellationToken cancellationToken)
        {
            var key = internalUri["storage://memory/".Length..];
            _files.Remove(key);
            return Task.CompletedTask;
        }
    }
}
