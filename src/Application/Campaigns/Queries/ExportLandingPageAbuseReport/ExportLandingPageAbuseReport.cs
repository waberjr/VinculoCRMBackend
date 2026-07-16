using System.Text;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageAbuseReport;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.Campaigns.Queries.ExportLandingPageAbuseReport;

public sealed record ExportLandingPageAbuseReportQuery(
    string Format,
    string? TargetType = null,
    Guid? TargetId = null,
    string? Source = null,
    bool? Blocked = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null) : IRequest<LandingPageAbuseReportExportDto>;

public sealed record LandingPageAbuseReportExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportLandingPageAbuseReportQueryHandler : IRequestHandler<ExportLandingPageAbuseReportQuery, LandingPageAbuseReportExportDto>
{
    private readonly ISender _sender;
    private readonly ILandingPageAbuseReportPdfExporter _pdfExporter;

    public ExportLandingPageAbuseReportQueryHandler(ISender sender, ILandingPageAbuseReportPdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<LandingPageAbuseReportExportDto> Handle(ExportLandingPageAbuseReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(
            new GetLandingPageAbuseReportQuery(request.TargetType, request.TargetId, request.Source, request.Blocked, request.StartDateUtc, request.EndDateUtc, Limit: 1000),
            cancellationToken);

        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new LandingPageAbuseReportExportDto("captacao-protecao.pdf", "application/pdf", _pdfExporter.Generate(report))
            : new LandingPageAbuseReportExportDto("captacao-protecao.csv", "text/csv", GenerateCsv(report));
    }

    private static byte[] GenerateCsv(LandingPageAbuseReportDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Resumo,Tentativas,Bloqueadas");
        builder.AppendLine($"Total,{report.AttemptsCount},{report.BlockedCount}");
        builder.AppendLine();
        builder.AppendLine("Data,Tipo,Landing,ID,Origem,Status,Motivo");
        foreach (var item in report.Items)
        {
            builder.AppendLine(string.Join(
                ',',
                item.AttemptedAtUtc.ToString("O"),
                Csv(item.TargetType),
                Csv(item.TargetName),
                item.TargetId,
                Csv(item.Source ?? string.Empty),
                item.Blocked ? "Bloqueada" : "Permitida",
                Csv(item.Reason ?? string.Empty)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
