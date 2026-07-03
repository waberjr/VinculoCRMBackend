using System;
using System.IO;
using VinculoBackend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VinculoBackend.Infrastructure.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("src/Web/appsettings.json", optional: true)
            .AddJsonFile($"src/Web/appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(Services.Database)
            ?? "Server=localhost;Port=3306;Database=VinculoBackendDb;User=admin;Password=password;";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySQL(connectionString)
            .Options;

        return new ApplicationDbContext(options, DesignTimeOrganizationContext.Instance);
    }

    private sealed class DesignTimeOrganizationContext : IOrganizationContext
    {
        public static readonly DesignTimeOrganizationContext Instance = new();

        public Guid OrganizationId => Guid.Empty;

        public bool HasOrganization => false;
    }
}
