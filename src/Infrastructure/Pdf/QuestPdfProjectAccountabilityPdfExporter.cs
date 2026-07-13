using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.ImpactProjects.Models;
using VinculoBackend.Application.ImpactProjects.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfProjectAccountabilityPdfExporter : IProjectAccountabilityPdfExporter
{
    static QuestPdfProjectAccountabilityPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(ProjectAccountabilityDto report)
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
                    column.Item().Text($"Prestacao de contas - {report.ProjectName}").FontSize(20).SemiBold();
                    column.Item().Text($"Filtro: {FilterSummary(report)}").FontColor(Colors.Grey.Darken2);
                    column.Item().Text($"Arrecadado: {report.RaisedAmount:C} | Meta: {report.GoalAmount:C} | Doadores: {report.DonorsCount} | Doacoes: {report.DonationsCount}");
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
                            header.Cell().AlignRight().Text("Doadores").SemiBold();
                            header.Cell().AlignRight().Text("Ticket medio").SemiBold();
                            header.Cell().AlignRight().Text("%").SemiBold();
                        });
                        foreach (var campaign in report.Campaigns)
                        {
                            table.Cell().Text(campaign.Name);
                            table.Cell().AlignRight().Text(campaign.RaisedAmount.ToString("C"));
                            table.Cell().AlignRight().Text(campaign.DonorsCount.ToString());
                            table.Cell().AlignRight().Text(campaign.AverageDonationAmount.ToString("C"));
                            table.Cell().AlignRight().Text($"{campaign.SharePercentage:N2}%");
                        }
                    });
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Doador").SemiBold();
                            header.Cell().Text("Campanha").SemiBold();
                            header.Cell().AlignRight().Text("Valor").SemiBold();
                            header.Cell().Text("Pagamento").SemiBold();
                        });
                        foreach (var donation in report.Donations)
                        {
                            table.Cell().Text(donation.DonorName);
                            table.Cell().Text(donation.CampaignName ?? "Sem campanha");
                            table.Cell().AlignRight().Text(donation.Amount.ToString("C"));
                            table.Cell().Text(donation.PaidAtUtc?.ToString("dd/MM/yyyy") ?? "-");
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    private static string FilterSummary(ProjectAccountabilityDto report)
    {
        var campaign = report.FilterCampaignName ?? "Todas as campanhas";
        var start = report.FilterStartDateUtc?.ToString("dd/MM/yyyy") ?? "inicio";
        var end = report.FilterEndDateUtc?.ToString("dd/MM/yyyy") ?? "hoje";

        return $"{campaign} | {start} a {end}";
    }
}
