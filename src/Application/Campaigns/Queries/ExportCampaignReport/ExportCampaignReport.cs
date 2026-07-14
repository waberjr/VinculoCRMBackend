using System.Text;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetCampaignReport;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.Campaigns.Queries.ExportCampaignReport;

public sealed record ExportCampaignReportQuery(
    string Format,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc,
    string? Status) : IRequest<CampaignReportExportDto>;

public sealed record CampaignReportExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportCampaignReportQueryHandler : IRequestHandler<ExportCampaignReportQuery, CampaignReportExportDto>
{
    private readonly ISender _sender;
    private readonly ICampaignReportPdfExporter _pdfExporter;

    public ExportCampaignReportQueryHandler(ISender sender, ICampaignReportPdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<CampaignReportExportDto> Handle(ExportCampaignReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetCampaignReportQuery(request.StartDateUtc, request.EndDateUtc, request.Status), cancellationToken);
        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new CampaignReportExportDto("campanhas-relatorio.pdf", "application/pdf", _pdfExporter.Generate(report))
            : new CampaignReportExportDto("campanhas-relatorio.csv", "text/csv", GenerateCsv(report));
    }

    private static byte[] GenerateCsv(CampaignReportDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Campanhas,Meta,Arrecadado,Doadores,Doacoes,Ticket medio");
        builder.AppendLine($"{report.CampaignsCount},{report.GoalAmount},{report.ConfirmedAmount},{report.DonorsCount},{report.DonationsCount},{report.AverageDonationAmount}");
        builder.AppendLine();
        builder.AppendLine("Campanha,Status,Meta,Arrecadado,Doadores,Doacoes,Ticket medio,Percentual meta");
        foreach (var campaign in report.Campaigns)
        {
            builder.AppendLine($"{Csv(campaign.Name)},{campaign.Status},{campaign.GoalAmount},{campaign.ConfirmedAmount},{campaign.DonorsCount},{campaign.DonationsCount},{campaign.AverageDonationAmount},{campaign.GoalPercentage}%");
        }

        builder.AppendLine();
        builder.AppendLine("Periodo,Arrecadado,Doadores,Doacoes");
        foreach (var period in report.Periods)
        {
            builder.AppendLine($"{period.Period},{period.ConfirmedAmount},{period.DonorsCount},{period.DonationsCount}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
