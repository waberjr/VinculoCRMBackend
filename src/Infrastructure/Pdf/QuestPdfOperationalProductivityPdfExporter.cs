using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivity;
using VinculoBackend.Application.OperationalAlerts.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfOperationalProductivityPdfExporter : IOperationalProductivityPdfExporter
{
    static QuestPdfOperationalProductivityPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(OperationalProductivityDto productivity)
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
                    column.Item().Text("Produtividade operacional").FontSize(20).SemiBold();
                    column.Item().Text($"Periodo: {productivity.StartDateUtc:dd/MM/yyyy} a {productivity.EndDateUtc:dd/MM/yyyy}");
                    column.Item().Text($"Criadas: {productivity.CreatedTasksCount} | Concluidas: {productivity.CompletedTasksCount} | Fora do prazo: {productivity.OverdueCompletedTasksCount} | Alertas resolvidos: {productivity.ResolvedAlertsCount}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Responsavel").SemiBold();
                            header.Cell().Text("Criadas").SemiBold();
                            header.Cell().Text("Concl.").SemiBold();
                            header.Cell().Text("Fora prazo").SemiBold();
                            header.Cell().Text("Alertas").SemiBold();
                            header.Cell().Text("Meta").SemiBold();
                            header.Cell().Text("SLA").SemiBold();
                        });

                        foreach (var item in productivity.Items.Take(80))
                        {
                            table.Cell().Text(item.AssignedUserName);
                            table.Cell().Text(item.CreatedTasksCount.ToString());
                            table.Cell().Text(item.CompletedTasksCount.ToString());
                            table.Cell().Text(item.OverdueCompletedTasksCount.ToString());
                            table.Cell().Text(item.ResolvedAlertsCount.ToString());
                            table.Cell().Text(item.OperationalTaskGoalMonthly?.ToString() ?? "-");
                            table.Cell().Text(item.OperationalSlaHours?.ToString() ?? "-");
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
