using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfLandingPagePerformancePdfExporter : ILandingPagePerformancePdfExporter
{
    static QuestPdfLandingPagePerformancePdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(LandingPagePerformanceDto report)
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
                    column.Item().Text("Desempenho das landings").FontSize(20).SemiBold();
                    column.Item().Text($"Views: {report.ViewsCount} | Leads: {report.LeadsCount} | Promessas: {report.PromisesCount} | Confirmadas: {report.ConfirmedDonationsCount} | Conversao: {report.ConversionRate:N2}%");
                    column.Item().Text($"Valor confirmado: {report.ConfirmedAmount:C}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Landing").SemiBold();
                            header.Cell().AlignRight().Text("Views").SemiBold();
                            header.Cell().AlignRight().Text("Leads").SemiBold();
                            header.Cell().AlignRight().Text("Confirmadas").SemiBold();
                            header.Cell().AlignRight().Text("Valor").SemiBold();
                            header.Cell().AlignRight().Text("%").SemiBold();
                        });

                        foreach (var item in report.Items.Take(20))
                        {
                            table.Cell().Text(item.TargetName);
                            table.Cell().AlignRight().Text(item.ViewsCount.ToString());
                            table.Cell().AlignRight().Text(item.LeadsCount.ToString());
                            table.Cell().AlignRight().Text(item.ConfirmedDonationsCount.ToString());
                            table.Cell().AlignRight().Text(item.ConfirmedAmount.ToString("C"));
                            table.Cell().AlignRight().Text($"{item.ConversionRate:N2}%");
                        }
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Origem").SemiBold();
                            header.Cell().AlignRight().Text("Views").SemiBold();
                            header.Cell().AlignRight().Text("Leads").SemiBold();
                        });

                        foreach (var source in report.Sources.Take(15))
                        {
                            table.Cell().Text(source.Source);
                            table.Cell().AlignRight().Text(source.ViewsCount.ToString());
                            table.Cell().AlignRight().Text(source.LeadsCount.ToString());
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
