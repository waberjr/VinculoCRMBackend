using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfLandingPageBlockRulesPdfExporter : ILandingPageBlockRulesPdfExporter
{
    static QuestPdfLandingPageBlockRulesPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(IReadOnlyCollection<LandingPageBlockRuleDto> rules)
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
                    column.Item().Text("Regras de bloqueio das landings").FontSize(20).SemiBold();
                    column.Item().Text($"Total: {rules.Count}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Criada").SemiBold();
                            header.Cell().Text("Landing").SemiBold();
                            header.Cell().Text("Regra").SemiBold();
                            header.Cell().Text("Expira").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                        });

                        foreach (var rule in rules.Take(60))
                        {
                            table.Cell().Text(rule.CreatedAtUtc.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Text(rule.TargetName);
                            table.Cell().Text(rule.FingerprintHash ?? rule.Source ?? "-");
                            table.Cell().Text(rule.ExpiresAtUtc?.ToString("dd/MM/yyyy HH:mm") ?? "Sem expiracao");
                            table.Cell().Text(rule.IsActive ? rule.IsExpired ? "Expirada" : "Ativa" : "Inativa");
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
