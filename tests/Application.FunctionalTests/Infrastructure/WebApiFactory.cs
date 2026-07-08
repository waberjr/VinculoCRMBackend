using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VinculoBackend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VinculoBackend.Application.FunctionalTests.Infrastructure;

public class WebApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseSetting("ConnectionStrings:VinculoBackendDb", connectionString);

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(provider =>
                {
                    var mock = new Mock<IUser>();
                    mock.SetupGet(x => x.Roles).Returns(TestApp.GetRoles());
                    mock.SetupGet(x => x.Id).Returns(TestApp.GetUserId());
                    return mock.Object;
                });

            services
                .RemoveAll<IOrganizationContext>()
                .AddTransient(provider =>
                {
                    var organizationId = TestApp.GetOrganizationId();
                    var mock = new Mock<IOrganizationContext>();
                    mock.SetupGet(x => x.OrganizationId).Returns(organizationId ?? Guid.Empty);
                    mock.SetupGet(x => x.HasOrganization).Returns(organizationId is not null);
                    return mock.Object;
                });

            services
                .RemoveAll<IFileStorageService>()
                .AddSingleton<IFileStorageService, InMemoryFileStorageService>();
        });
    }

    private sealed class InMemoryFileStorageService : IFileStorageService
    {
        private readonly Dictionary<string, StoredFileDownload> _files = [];

        public async Task<StoredFileReference> StoreAsync(StoreFileRequest request, CancellationToken cancellationToken)
        {
            var key = Guid.NewGuid().ToString("N");
            var content = new MemoryStream();
            await request.Content.CopyToAsync(content, cancellationToken);
            content.Position = 0;
            _files[key] = new StoredFileDownload(request.FileName, request.ContentType, content);
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

        public Task<TemporaryFileAccessUrl?> CreateTemporaryReadUrlAsync(string internalUri, TimeSpan ttl, CancellationToken cancellationToken)
        {
            var key = internalUri["storage://memory/".Length..];
            return Task.FromResult(_files.ContainsKey(key)
                ? new TemporaryFileAccessUrl($"https://storage.local/{key}", DateTimeOffset.UtcNow.Add(ttl))
                : null);
        }
    }
}
