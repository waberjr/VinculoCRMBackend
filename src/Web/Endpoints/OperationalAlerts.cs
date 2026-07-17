using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.AcknowledgeOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Commands.AssignOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Commands.ResolveOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.ExportOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertsSummary;

namespace VinculoBackend.Web.Endpoints;

public sealed class OperationalAlerts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetAlerts);
        groupBuilder.MapGet(GetSummary, "Summary");
        groupBuilder.MapGet(ExportAlerts, "Export");
        groupBuilder.MapPost(AssignAlert, "{id}/Assign");
        groupBuilder.MapPost(AcknowledgeAlert, "{id}/Acknowledge");
        groupBuilder.MapPost(ResolveAlert, "{id}/Resolve");
    }

    public static async Task<Ok<PaginatedResult<OperationalAlertDto>>> GetAlerts(
        ISender sender,
        string? search,
        string? severity,
        string? status,
        string? source,
        string? relatedEntityType,
        Guid? relatedEntityId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertsQuery
        {
            Search = search,
            Severity = severity,
            Status = status,
            Source = source,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
            PageNumber = pageNumber <= 0 ? 1 : pageNumber,
            PageSize = pageSize <= 0 ? 20 : pageSize,
        }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<OperationalAlertsSummaryDto>> GetSummary(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertsSummaryQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<FileContentHttpResult> ExportAlerts(
        ISender sender,
        string? format,
        string? search,
        string? severity,
        string? status,
        string? source,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportOperationalAlertsQuery(format ?? "csv", search, severity, status, source, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<NoContent> AssignAlert(ISender sender, Guid id, AssignOperationalAlertCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> AcknowledgeAlert(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new AcknowledgeOperationalAlertCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ResolveAlert(ISender sender, Guid id, ResolveOperationalAlertCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }
}
