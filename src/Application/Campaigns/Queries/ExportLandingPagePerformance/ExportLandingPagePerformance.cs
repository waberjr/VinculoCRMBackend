using System.Globalization;
using System.Text;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.Campaigns.Queries.ExportLandingPagePerformance;

public sealed record ExportLandingPagePerformanceQuery(
    string Format,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null,
    string? TargetType = null,
    string? Source = null) : IRequest<LandingPagePerformanceExportDto>;

public sealed record LandingPagePerformanceExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportLandingPagePerformanceQueryHandler : IRequestHandler<ExportLandingPagePerformanceQuery, LandingPagePerformanceExportDto>
{
    private readonly ISender _sender;
    private readonly ILandingPagePerformancePdfExporter _pdfExporter;

    public ExportLandingPagePerformanceQueryHandler(ISender sender, ILandingPagePerformancePdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<LandingPagePerformanceExportDto> Handle(ExportLandingPagePerformanceQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetLandingPagePerformanceQuery(request.StartDateUtc, request.EndDateUtc, request.TargetType, request.Source), cancellationToken);
        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new LandingPagePerformanceExportDto("captacao-desempenho.pdf", "application/pdf", _pdfExporter.Generate(report))
            : new LandingPagePerformanceExportDto("captacao-desempenho.csv", "text/csv", GenerateCsv(report));
    }

    private static byte[] GenerateCsv(LandingPagePerformanceDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Resumo,Visualizacoes,Leads,Promessas,Confirmadas,Valor confirmado,Conversao");
        builder.AppendLine($"Total,{report.ViewsCount},{report.LeadsCount},{report.PromisesCount},{report.ConfirmedDonationsCount},{Money(report.ConfirmedAmount)},{report.ConversionRate}%");

        builder.AppendLine();
        builder.AppendLine("Landing,Tipo,Titulo,Publicada,Visualizacoes,Leads,Promessas,Confirmadas,Valor confirmado,Conversao,Origem principal");
        foreach (var item in report.Items)
        {
            builder.AppendLine(string.Join(
                ',',
                Csv(item.TargetName),
                Csv(item.TargetType),
                Csv(item.Title),
                item.IsPublished ? "Sim" : "Nao",
                item.ViewsCount,
                item.LeadsCount,
                item.PromisesCount,
                item.ConfirmedDonationsCount,
                Money(item.ConfirmedAmount),
                $"{item.ConversionRate}%",
                Csv(item.TopSource)));
        }

        builder.AppendLine();
        builder.AppendLine("Origem,Visualizacoes,Leads");
        foreach (var source in report.Sources)
        {
            builder.AppendLine($"{Csv(source.Source)},{source.ViewsCount},{source.LeadsCount}");
        }

        builder.AppendLine();
        builder.AppendLine("UTM Source,UTM Medium,UTM Campaign,Visualizacoes,Leads");
        foreach (var utm in report.Utms)
        {
            builder.AppendLine($"{Csv(utm.UtmSource)},{Csv(utm.UtmMedium)},{Csv(utm.UtmCampaign)},{utm.ViewsCount},{utm.LeadsCount}");
        }

        builder.AppendLine();
        builder.AppendLine("Data,Visualizacoes,Leads");
        foreach (var day in report.Daily)
        {
            builder.AppendLine($"{day.Date},{day.ViewsCount},{day.LeadsCount}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
