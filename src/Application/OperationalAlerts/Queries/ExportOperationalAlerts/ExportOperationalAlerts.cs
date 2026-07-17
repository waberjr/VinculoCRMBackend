using System.Text;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlerts;
using VinculoBackend.Application.OperationalAlerts.Services;

namespace VinculoBackend.Application.OperationalAlerts.Queries.ExportOperationalAlerts;

public sealed record ExportOperationalAlertsQuery(
    string Format,
    string? Search = null,
    string? Severity = null,
    string? Status = null,
    string? Source = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null) : IRequest<OperationalAlertsExportDto>;

public sealed record OperationalAlertsExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportOperationalAlertsQueryHandler : IRequestHandler<ExportOperationalAlertsQuery, OperationalAlertsExportDto>
{
    private readonly ISender _sender;
    private readonly IOperationalAlertsPdfExporter _pdfExporter;

    public ExportOperationalAlertsQueryHandler(ISender sender, IOperationalAlertsPdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<OperationalAlertsExportDto> Handle(ExportOperationalAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _sender.Send(new GetOperationalAlertsQuery
        {
            Search = request.Search,
            Severity = request.Severity,
            Status = request.Status,
            Source = request.Source,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            PageNumber = 1,
            PageSize = 1000,
        }, cancellationToken);

        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new OperationalAlertsExportDto("alertas-operacionais.pdf", "application/pdf", _pdfExporter.Generate(alerts.Items))
            : new OperationalAlertsExportDto("alertas-operacionais.csv", "text/csv", GenerateCsv(alerts.Items));
    }

    private static byte[] GenerateCsv(IEnumerable<OperationalAlertDto> alerts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Data,Titulo,Descricao,Severidade,Status,Origem,Responsavel,Prazo,Resolucao");
        foreach (var alert in alerts)
        {
            builder.AppendLine(string.Join(
                ',',
                alert.OccurredAtUtc.ToString("O"),
                Csv(alert.Title),
                Csv(alert.Description ?? string.Empty),
                alert.Severity,
                alert.Status,
                Csv(alert.Source),
                Csv(alert.AssignedUserId ?? string.Empty),
                alert.DueAtUtc?.ToString("O") ?? string.Empty,
                Csv(alert.ResolutionNote ?? string.Empty)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
