using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfCampaignReportPdfExporter : ICampaignReportPdfExporter
{
    static QuestPdfCampaignReportPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(CampaignReportDto report)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(style => style.FontSize(10));
                page.Content().Column(column =>
                {
                    column.Spacing(14);
                    column.Item().Text("Relatorio consolidado de campanhas").FontSize(20).SemiBold();
                    column.Item().Text($"Arrecadado: {report.ConfirmedAmount:C} | Meta: {report.GoalAmount:C} | Doadores: {report.DonorsCount} | Doacoes: {report.DonationsCount}");
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Campanha").SemiBold();
                            header.Cell().AlignRight().Text("Arrecadado").SemiBold();
                            header.Cell().AlignRight().Text("Meta").SemiBold();
                            header.Cell().AlignRight().Text("Doadores").SemiBold();
                            header.Cell().AlignRight().Text("%").SemiBold();
                        });
                        foreach (var campaign in report.Campaigns)
                        {
                            table.Cell().Text(campaign.Name);
                            table.Cell().AlignRight().Text(campaign.ConfirmedAmount.ToString("C"));
                            table.Cell().AlignRight().Text(campaign.GoalAmount.ToString("C"));
                            table.Cell().AlignRight().Text(campaign.DonorsCount.ToString());
                            table.Cell().AlignRight().Text($"{campaign.GoalPercentage:N2}%");
                        }
                    });
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Periodo").SemiBold();
                            header.Cell().AlignRight().Text("Arrecadado").SemiBold();
                            header.Cell().AlignRight().Text("Doadores").SemiBold();
                            header.Cell().AlignRight().Text("Doacoes").SemiBold();
                        });
                        foreach (var period in report.Periods)
                        {
                            table.Cell().Text(period.Period);
                            table.Cell().AlignRight().Text(period.ConfirmedAmount.ToString("C"));
                            table.Cell().AlignRight().Text(period.DonorsCount.ToString());
                            table.Cell().AlignRight().Text(period.DonationsCount.ToString());
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
