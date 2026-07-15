using Azure.Identity;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Infrastructure.Data;
using VinculoBackend.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    public static void AddWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
        builder.Services.AddScoped<IReceiptHtmlRenderer, ReceiptHtmlRenderer>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("PublicLandingLead", httpContext =>
            {
                var kind = httpContext.Request.RouteValues["kind"]?.ToString() ?? "unknown";
                var id = httpContext.Request.RouteValues["id"]?.ToString() ?? "unknown";
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"{kind}:{id}:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(10),
                    });
            });
        });

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<IdentityApiOperationTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }

                policy.AllowAnyMethod().AllowAnyHeader();
            });
        });
    }

    public static void AddKeyVaultIfConfigured(this WebApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
