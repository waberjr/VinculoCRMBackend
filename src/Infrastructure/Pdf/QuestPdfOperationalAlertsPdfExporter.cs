using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfOperationalAlertsPdfExporter : IOperationalAlertsPdfExporter
{
    static QuestPdfOperationalAlertsPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(IReadOnlyCollection<OperationalAlertDto> alerts)
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
                    column.Item().Text("Alertas operacionais").FontSize(20).SemiBold();
                    column.Item().Text($"Total: {alerts.Count}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(2.4f);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(1.4f);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Data").SemiBold();
                            header.Cell().Text("Alerta").SemiBold();
                            header.Cell().Text("Sev.").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                            header.Cell().Text("Prazo").SemiBold();
                        });

                        foreach (var alert in alerts.Take(60))
                        {
                            table.Cell().Text(alert.OccurredAtUtc.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Text(alert.Title);
                            table.Cell().Text(alert.Severity.ToString());
                            table.Cell().Text(alert.Status.ToString());
                            table.Cell().Text(alert.DueAtUtc?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
