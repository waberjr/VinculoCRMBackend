using VinculoBackend.Application.Campaigns.Commands.ActivateCampaign;
using VinculoBackend.Application.Campaigns.Commands.CancelCampaign;
using VinculoBackend.Application.Campaigns.Commands.CompleteCampaign;
using VinculoBackend.Application.Campaigns.Commands.CreateCampaign;
using VinculoBackend.Application.Campaigns.Commands.UpdateCampaign;
using VinculoBackend.Application.Campaigns.Commands.UpsertLandingPageConfiguration;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetCampaignReport;
using VinculoBackend.Application.Campaigns.Queries.ExportCampaignReport;
using VinculoBackend.Application.Campaigns.Queries.GetPublicLandingPage;
using VinculoBackend.Application.Campaigns.Queries.GetCampaigns;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageConfiguration;
using VinculoBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Campaigns : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetCampaigns);
        groupBuilder.MapGet(GetCampaignReport, "Report");
        groupBuilder.MapGet(ExportCampaignReport, "Report/Export");
        groupBuilder.MapGet(GetLandingConfiguration, "Landing/{targetType}/{targetId}");
        groupBuilder.MapPost(CreateCampaign);
        groupBuilder.MapPut(UpsertLandingConfiguration, "Landing/{targetType}/{targetId}");
        groupBuilder.MapPut(UpdateCampaign, "{id}");
        groupBuilder.MapPost(ActivateCampaign, "{id}/Activate");
        groupBuilder.MapPost(CompleteCampaign, "{id}/Complete");
        groupBuilder.MapPost(CancelCampaign, "{id}/Cancel");
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

    public static async Task<Ok<LandingPageConfigurationDto>> UpsertLandingConfiguration(
        ISender sender,
        string targetType,
        Guid targetId,
        UpsertLandingPageConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { TargetType = targetType, TargetId = targetId }, cancellationToken);
        return TypedResults.Ok(result);
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
}
