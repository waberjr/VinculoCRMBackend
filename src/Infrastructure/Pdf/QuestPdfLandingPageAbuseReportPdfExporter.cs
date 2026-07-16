using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfLandingPageAbuseReportPdfExporter : ILandingPageAbuseReportPdfExporter
{
    static QuestPdfLandingPageAbuseReportPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(LandingPageAbuseReportDto report)
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
                    column.Item().Text("Protecao das landings").FontSize(20).SemiBold();
                    column.Item().Text($"Tentativas: {report.AttemptsCount} | Bloqueadas: {report.BlockedCount}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Data").SemiBold();
                            header.Cell().Text("Landing").SemiBold();
                            header.Cell().Text("Origem").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                            header.Cell().Text("Motivo").SemiBold();
                        });

                        foreach (var item in report.Items.Take(40))
                        {
                            table.Cell().Text(item.AttemptedAtUtc.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Text(item.TargetName);
                            table.Cell().Text(item.Source ?? "-");
                            table.Cell().Text(item.Blocked ? "Bloqueada" : "Permitida");
                            table.Cell().Text(item.Reason ?? "-");
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
