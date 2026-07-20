using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.OperationalAlerts.Commands.AddOperationalAlertNote;
using VinculoBackend.Application.OperationalAlerts.Commands.BulkAcknowledgeOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Commands.CreateTasksFromOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Commands.UpdateOperationalProductivityGoal;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Commands.AcknowledgeOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Commands.AssignOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Commands.ResolveOperationalAlert;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertIds;
using VinculoBackend.Application.OperationalAlerts.Queries.ExportOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertAudit;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertDetail;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertRules;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertsSummary;
using VinculoBackend.Application.OperationalAlerts.Queries.ExportOperationalProductivity;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivity;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivityGoalAudit;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalWorkload;
using VinculoBackend.Application.OperationalAlerts.Commands.UpsertOperationalAlertRule;

namespace VinculoBackend.Web.Endpoints;

public sealed class OperationalAlerts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetAlerts);
        groupBuilder.MapGet(GetAlertIds, "Ids");
        groupBuilder.MapGet(GetSummary, "Summary");
        groupBuilder.MapGet(GetWorkload, "Workload");
        groupBuilder.MapGet(GetProductivity, "Productivity");
        groupBuilder.MapGet(GetProductivityGoalAudit, "Productivity/Goals/Audit");
        groupBuilder.MapGet(ExportProductivity, "Productivity/Export");
        groupBuilder.MapGet(GetRules, "Rules");
        groupBuilder.MapGet(GetDetail, "{id}");
        groupBuilder.MapGet(GetAudit, "{id}/Audit");
        groupBuilder.MapGet(ExportAlerts, "Export");
        groupBuilder.MapPut(UpsertRule, "Rules/{source}");
        groupBuilder.MapPost(AssignAlert, "{id}/Assign");
        groupBuilder.MapPost(AcknowledgeAlert, "{id}/Acknowledge");
        groupBuilder.MapPost(BulkAcknowledge, "BulkAcknowledge");
        groupBuilder.MapPost(CreateTasksFromAlerts, "BulkCreateTasks");
        groupBuilder.MapPut(UpdateProductivityGoal, "Productivity/Goals/{userId}");
        groupBuilder.MapPost(AddNote, "{id}/Notes");
        groupBuilder.MapPost(ResolveAlert, "{id}/Resolve");
    }

    public static async Task<Ok<IReadOnlyCollection<OperationalWorkloadItemDto>>> GetWorkload(
        ISender sender,
        string? assignedUserId,
        string? source,
        bool? overdueOnly,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalWorkloadQuery(assignedUserId, source, overdueOnly), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<OperationalProductivityDto>> GetProductivity(
        ISender sender,
        string? assignedUserId,
        string? source,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalProductivityQuery(assignedUserId, source, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<FileContentHttpResult> ExportProductivity(
        ISender sender,
        string? format,
        string? assignedUserId,
        string? source,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportOperationalProductivityQuery(format ?? "csv", assignedUserId, source, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<Ok<IReadOnlyCollection<OperationalProductivityGoalAuditEntryDto>>> GetProductivityGoalAudit(
        ISender sender,
        string? userId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalProductivityGoalAuditQuery(userId), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<Guid>>> GetAlertIds(
        ISender sender,
        string? search,
        string? severity,
        string? status,
        string? source,
        string? assignedUserId,
        bool? overdueOnly,
        string? relatedEntityType,
        Guid? relatedEntityId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertIdsQuery
        {
            Search = search,
            Severity = severity,
            Status = status,
            Source = source,
            AssignedUserId = assignedUserId,
            OverdueOnly = overdueOnly,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
        }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PaginatedResult<OperationalAlertDto>>> GetAlerts(
        ISender sender,
        string? search,
        string? severity,
        string? status,
        string? source,
        string? assignedUserId,
        bool? overdueOnly,
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
            AssignedUserId = assignedUserId,
            OverdueOnly = overdueOnly,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
            PageNumber = pageNumber <= 0 ? 1 : pageNumber,
            PageSize = pageSize <= 0 ? 20 : pageSize,
        }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<OperationalAlertRuleDto>>> GetRules(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertRulesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<OperationalAlertDetailDto>> GetDetail(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertDetailQuery(id), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<OperationalAlertAuditEntryDto>>> GetAudit(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOperationalAlertAuditQuery(id), cancellationToken);
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
        string? assignedUserId,
        bool? overdueOnly,
        string? relatedEntityType,
        Guid? relatedEntityId,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExportOperationalAlertsQuery(format ?? "csv", search, severity, status, source, assignedUserId, overdueOnly, relatedEntityType, relatedEntityId, startDateUtc, endDateUtc), cancellationToken);
        return TypedResults.File(result.Content, result.ContentType, result.FileName);
    }

    public static async Task<NoContent> UpsertRule(ISender sender, string source, UpsertOperationalAlertRuleCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Source = source }, cancellationToken);
        return TypedResults.NoContent();
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

    public static async Task<Ok<int>> BulkAcknowledge(ISender sender, BulkAcknowledgeOperationalAlertsCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<int>> CreateTasksFromAlerts(ISender sender, CreateTasksFromOperationalAlertsCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<NoContent> UpdateProductivityGoal(ISender sender, string userId, UpdateOperationalProductivityGoalCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { UserId = userId }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> AddNote(ISender sender, Guid id, AddOperationalAlertNoteCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ResolveAlert(ISender sender, Guid id, ResolveOperationalAlertCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }
}
