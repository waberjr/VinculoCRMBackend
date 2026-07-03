using System;
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
        });
    }
}
