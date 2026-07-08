using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Infrastructure.Data.Interceptors;
using VinculoBackend.Infrastructure.Documents;
using VinculoBackend.Infrastructure.Identity;
using VinculoBackend.Infrastructure.Locations;
using VinculoBackend.Infrastructure.Organizations;
using VinculoBackend.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"String de conexão '{Services.Database}' não encontrada.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMySQL(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.Configure<AzureBlobFileStorageOptions>(configuration.GetSection(AzureBlobFileStorageOptions.SectionName));
        services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddHttpClient<ILocationLookupService, IbgeLocationLookupService>(client =>
        {
            client.BaseAddress = new Uri("https://servicodados.ibge.gov.br/api/v1/");
        });

        services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IBrazilianDocumentValidator, DocsBrBrazilianDocumentValidator>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddScoped<IOrganizationAdministrationService, OrganizationAdministrationService>();
    }
}
