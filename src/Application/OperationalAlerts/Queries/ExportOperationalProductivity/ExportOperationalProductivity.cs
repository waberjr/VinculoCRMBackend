using System.Text;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivity;
using VinculoBackend.Application.OperationalAlerts.Services;

namespace VinculoBackend.Application.OperationalAlerts.Queries.ExportOperationalProductivity;

public sealed record ExportOperationalProductivityQuery(
    string Format,
    string? AssignedUserId,
    string? Source,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc) : IRequest<OperationalProductivityExportDto>;

public sealed record OperationalProductivityExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportOperationalProductivityQueryHandler : IRequestHandler<ExportOperationalProductivityQuery, OperationalProductivityExportDto>
{
    private readonly IOperationalProductivityPdfExporter _pdfExporter;
    private readonly ISender _sender;

    public ExportOperationalProductivityQueryHandler(IOperationalProductivityPdfExporter pdfExporter, ISender sender)
    {
        _pdfExporter = pdfExporter;
        _sender = sender;
    }

    public async Task<OperationalProductivityExportDto> Handle(ExportOperationalProductivityQuery request, CancellationToken cancellationToken)
    {
        var productivity = await _sender.Send(new GetOperationalProductivityQuery(
            request.AssignedUserId,
            request.Source,
            request.StartDateUtc,
            request.EndDateUtc), cancellationToken);

        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new OperationalProductivityExportDto("produtividade-operacional.pdf", "application/pdf", _pdfExporter.Generate(productivity))
            : new OperationalProductivityExportDto("produtividade-operacional.csv", "text/csv", GenerateCsv(productivity));
    }

    private static byte[] GenerateCsv(OperationalProductivityDto productivity)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Inicio,Fim,Tarefas criadas,Tarefas concluidas,Concluidas fora do prazo,Alertas resolvidos");
        builder.AppendLine($"{productivity.StartDateUtc:O},{productivity.EndDateUtc:O},{productivity.CreatedTasksCount},{productivity.CompletedTasksCount},{productivity.OverdueCompletedTasksCount},{productivity.ResolvedAlertsCount}");
        builder.AppendLine();
        builder.AppendLine("Responsavel,Tarefas criadas,Tarefas concluidas,Concluidas fora do prazo,Alertas resolvidos,Meta mensal,SLA horas");
        foreach (var item in productivity.Items)
        {
            builder.AppendLine(string.Join(
                ',',
                Csv(item.AssignedUserName),
                item.CreatedTasksCount,
                item.CompletedTasksCount,
                item.OverdueCompletedTasksCount,
                item.ResolvedAlertsCount,
                item.OperationalTaskGoalMonthly?.ToString() ?? string.Empty,
                item.OperationalSlaHours?.ToString() ?? string.Empty));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
