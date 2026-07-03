using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using VinculoBackend.Shared;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddMySql(Services.DatabaseServer)
    .WithLifetime(ContainerLifetime.Persistent);

var database = databaseServer.AddDatabase(Services.Database);

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

web.WithAspNetCoreEnvironment();

builder.Build().Run();
