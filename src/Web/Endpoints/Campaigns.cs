using VinculoBackend.Application.Campaigns.Commands.ActivateCampaign;
using VinculoBackend.Application.Campaigns.Commands.CancelCampaign;
using VinculoBackend.Application.Campaigns.Commands.CloneLandingPageTemplate;
using VinculoBackend.Application.Campaigns.Commands.CompleteCampaign;
using VinculoBackend.Application.Campaigns.Commands.CreateCampaign;
using VinculoBackend.Application.Campaigns.Commands.CreateLandingPageTemplate;
using VinculoBackend.Application.Campaigns.Commands.DeleteLandingPageTemplate;
using VinculoBackend.Application.Campaigns.Commands.SetLandingPageTemplateActive;
using VinculoBackend.Application.Campaigns.Commands.UpdateCampaign;
using VinculoBackend.Application.Campaigns.Commands.UpdateLandingPageTemplate;
using VinculoBackend.Application.Campaigns.Commands.UpsertLandingPageConfiguration;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetCampaignReport;
using VinculoBackend.Application.Campaigns.Queries.ExportCampaignReport;
using VinculoBackend.Application.Campaigns.Queries.ExportLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Queries.ExportLandingPageLeads;
using VinculoBackend.Application.Campaigns.Queries.GetPublicLandingPage;
using VinculoBackend.Application.Campaigns.Queries.GetCampaigns;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageAudit;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageConfiguration;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageLeads;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageMetrics;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageTemplates;
using VinculoBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace VinculoBackend.Web.Endpoints;

public sealed class Campaigns : IEndpointGroup
{
    public sealed class LandingPageConfigurationFormRequest
    {
        public string Title { get; init; } = string.Empty;

        public string? Subtitle { get; init; }

        public string? HeroImageUrl { get; init; }

        public decimal? GoalAmount { get; init; }

        public bool IsActive { get; init; } = true;

        public bool IsPublished { get; init; }

        public string? CustomFieldsJson { get; init; }

        public IFormFile? HeroImage { get; init; }
    }

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetCampaigns);
        groupBuilder.MapGet(GetCampaignReport, "Report");
        groupBuilder.MapGet(ExportCampaignReport, "Report/Export");
        groupBuilder.MapGet(GetLandingConfiguration, "Landing/{targetType}/{targetId}");
        groupBuilder.MapGet(GetLandingTemplates, "Landing/Templates");
        groupBuilder.MapGet(GetLandingAudit, "Landing/Audit");
        groupBuilder.MapGet(GetLandingPerformance, "Landing/Performance");
        groupBuilder.MapGet(ExportLandingPerformance, "Landing/Performance/Export");
        groupBuilder.MapGet(PreviewLanding, "Landing/{targetType}/{targetId}/Preview");
        groupBuilder.MapGet(GetLandingMetrics, "Landing/{targetType}/{targetId}/Metrics");
        groupBuilder.MapGet(GetLandingLeads, "Landing/{targetType}/{targetId}/Leads");
        groupBuilder.MapGet(ExportLandingLeads, "Landing/{targetType}/{targetId}/Leads/Export");
        groupBuilder.MapPost(CreateCampaign);
        groupBuilder.MapPost(CreateLandingTemplate, "Landing/Templates");
        groupBuilder.MapPost(CloneLandingTemplate, "Landing/Templates/{id}/Clone");
        groupBuilder.MapPut(UpdateLandingTemplate, "Landing/Templates/{id}");
        groupBuilder.MapPost(ActivateLandingTemplate, "Landing/Templates/{id}/Activate");
        groupBuilder.MapPost(DeactivateLandingTemplate, "Landing/Templates/{id}/Deactivate");
        groupBuilder.MapDelete(DeleteLandingTemplate, "Landing/Templates/{id}");
        groupBuilder.MapPut(UpsertLandingConfiguration, "Landing/{targetType}/{targetId}").DisableAntiforgery();
        groupBuilder.MapPut(UpdateCampaign, "{id}");
        groupBuilder.MapPost(ActivateCampaign, "{id}/Activate");
        groupBuilder.MapPost(CompleteCampaign, "{id}/Complete");
        groupBuilder.MapPost(CancelCampaign, "{id}/Cancel");
    }

    public static async Task<Ok<IReadOnlyCollection<LandingPageTemplateDto>>> GetLandingTemplates(
        ISender sender,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPageTemplatesQuery(includeInactive), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<LandingPageAuditEntryDto>>> GetLandingAudit(
        ISender sender,
        string? entityType,
        Guid? entityId,
        string? action,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPageAuditQuery(entityType, entityId, action, limit), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<LandingPagePerformanceDto>> GetLandingPerformance(
        ISender sender,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        string? targetType,
        string? source,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPagePerformanceQuery(startDateUtc, endDateUtc, targetType, source), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<FileContentHttpResult> ExportLandingPerformance(
        ISender sender,
        string? format,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        string? targetType,
        string? source,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportLandingPagePerformanceQuery(format ?? "csv", startDateUtc, endDateUtc, targetType, source), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<Ok<CampaignReportDto>> GetCampaignReport(
        ISender sender,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        string? status,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCampaignReportQuery(startDateUtc, endDateUtc, status), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<FileContentHttpResult> ExportCampaignReport(
        ISender sender,
        string? format,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        string? status,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportCampaignReportQuery(format ?? "csv", startDateUtc, endDateUtc, status), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<Results<Ok<LandingPageConfigurationDto>, NotFound>> GetLandingConfiguration(
        ISender sender,
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPageConfigurationQuery(targetType, targetId), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<PublicLandingPageDto>, NotFound>> PreviewLanding(
        ISender sender,
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicLandingPageQuery(targetType, targetId, IncludeDraft: true), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<LandingPageMetricsDto>> GetLandingMetrics(
        ISender sender,
        string targetType,
        Guid targetId,
        string? source,
        string? status,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPageMetricsQuery(targetType, targetId, source, status, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PaginatedResult<LandingPageLeadDto>>> GetLandingLeads(
        ISender sender,
        string targetType,
        Guid targetId,
        string? source,
        string? status,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLandingPageLeadsQuery(targetType, targetId, source, status, startDateUtc, endDateUtc, pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<FileContentHttpResult> ExportLandingLeads(
        ISender sender,
        string targetType,
        Guid targetId,
        string? source,
        string? status,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportLandingPageLeadsQuery(targetType, targetId, source, status, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<Ok<LandingPageConfigurationDto>> UpsertLandingConfiguration(
        ISender sender,
        string targetType,
        Guid targetId,
        [FromForm] LandingPageConfigurationFormRequest form,
        CancellationToken cancellationToken)
    {
        await using var content = form.HeroImage?.OpenReadStream();
        var result = await sender.Send(
            new UpsertLandingPageConfigurationCommand
            {
                TargetType = targetType,
                TargetId = targetId,
                Title = form.Title,
                Subtitle = form.Subtitle,
                HeroImageUrl = form.HeroImageUrl,
                GoalAmount = form.GoalAmount,
                IsActive = form.IsActive,
                IsPublished = form.IsPublished,
                CustomFields = ParseCustomFields(form.CustomFieldsJson),
                HeroImage = ToFileUpload(form.HeroImage, content),
            },
            cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateLandingTemplate(
        ISender sender,
        CreateLandingPageTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/Campaigns/Landing/Templates/{id}", id);
    }

    public static async Task<Created<Guid>> CloneLandingTemplate(
        ISender sender,
        Guid id,
        CloneLandingPageTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var cloneId = await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.Created($"/api/Campaigns/Landing/Templates/{cloneId}", cloneId);
    }

    public static async Task<NoContent> UpdateLandingTemplate(
        ISender sender,
        Guid id,
        UpdateLandingPageTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ActivateLandingTemplate(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SetLandingPageTemplateActiveCommand(id, true), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeactivateLandingTemplate(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SetLandingPageTemplateActiveCommand(id, false), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteLandingTemplate(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteLandingPageTemplateCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<PublicLandingPageDto>, NotFound>> Landing(
        ISender sender,
        string kind,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicLandingPageQuery(kind, id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<PaginatedResult<CampaignListItemDto>>> GetCampaigns(
        ISender sender,
        string? search,
        string? status,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetCampaignsQuery
        {
            Search = search,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateCampaign(ISender sender, CreateCampaignCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Campaigns/{id}", id);
    }

    public static async Task<NoContent> UpdateCampaign(ISender sender, Guid id, UpdateCampaignCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ActivateCampaign(ISender sender, Guid id)
    {
        await sender.Send(new ActivateCampaignCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CompleteCampaign(ISender sender, Guid id)
    {
        await sender.Send(new CompleteCampaignCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelCampaign(ISender sender, Guid id)
    {
        await sender.Send(new CancelCampaignCommand(id));
        return TypedResults.NoContent();
    }

    private static FileUpload? ToFileUpload(IFormFile? file, Stream? content)
    {
        return file is null || content is null
            ? null
            : new FileUpload(file.FileName, file.ContentType, content, file.Length);
    }

    private static IReadOnlyCollection<LandingPageCustomFieldDto> ParseCustomFields(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyCollection<LandingPageCustomFieldDto>>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
