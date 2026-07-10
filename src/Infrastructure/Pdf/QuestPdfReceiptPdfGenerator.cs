using VinculoBackend.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VinculoBackend.Application.Receipts.Models;

namespace VinculoBackend.Infrastructure.Pdf;

public sealed class QuestPdfReceiptPdfGenerator : IReceiptPdfGenerator
{
    static QuestPdfReceiptPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(ReceiptPrintDto receipt, ReceiptPdfLogo? logo)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontSize(10).FontColor(Colors.Grey.Darken4));

                page.Content().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(24).Column(column =>
                {
                    column.Spacing(18);
                    column.Item().Element(container => ComposeHeader(container, receipt, logo));
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Element(container => ComposeParty(container, "ORGANIZACAO", receipt.OrganizationName, receipt.OrganizationDocument));
                    column.Item().Element(container => ComposeParty(container, "DOADOR", receipt.DonorName, receipt.DonorDocument));
                    column.Item().Element(container => ComposeDonation(container, receipt));
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Text("Declaramos o recebimento da contribuicao descrita acima.").FontSize(11);
                    column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(box =>
                    {
                        box.Spacing(3);
                        box.Item().Text($"Codigo de verificacao: {VerificationCode(receipt.Id)}");
                        box.Item().Text($"Validacao: /api/Receipts/{receipt.Id}/Validate?code={VerificationCode(receipt.Id)}");
                        box.Item().Text($"Identificador do recibo: {receipt.Id}");
                    });
                    column.Item().Text("Este documento foi gerado eletronicamente pelo Vinculo CRM Filantropico.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, ReceiptPrintDto receipt, ReceiptPdfLogo? logo)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("RECIBO DE DOACAO").FontSize(20).SemiBold();
                column.Item().Text(receipt.Number).FontSize(13).FontColor(Colors.Grey.Darken2);
                column.Item().Text("Documento financeiro para registro e prestacao de contas.").FontSize(10).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(110).Height(54).AlignRight().AlignMiddle().Element(content =>
            {
                if (logo is null)
                {
                    content.Border(1).BorderColor(Colors.Grey.Lighten2).AlignCenter().AlignMiddle().Text(Fit(receipt.OrganizationName, 16)).FontSize(8);
                }
                else
                {
                    content.Image(logo.Content).FitArea();
                }
            });
        });
    }

    private static void ComposeParty(IContainer container, string title, string name, string? document)
    {
        container.Column(column =>
        {
            column.Spacing(4);
            column.Item().Text(title).FontSize(9).SemiBold().FontColor(Colors.Green.Darken2);
            column.Item().Text(name).FontSize(13).SemiBold();
            column.Item().Text(string.IsNullOrWhiteSpace(document) ? "Documento nao informado" : $"Documento: {document}")
                .FontColor(Colors.Grey.Darken1);
        });
    }

    private static void ComposeDonation(IContainer container, ReceiptPrintDto receipt)
    {
        container.Column(column =>
        {
            column.Spacing(10);
            column.Item().Text("VALOR RECEBIDO").FontSize(9).SemiBold().FontColor(Colors.Green.Darken2);
            column.Item().Text(receipt.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))).FontSize(24).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                AddInfo(table, "Pagamento", receipt.PaidAtUtc.ToString("dd/MM/yyyy"));
                AddInfo(table, "Emissao", receipt.IssuedAtUtc.ToString("dd/MM/yyyy HH:mm"));
                AddInfo(table, "Referencia", receipt.DonationReference);
                AddInfo(table, "Campanha", receipt.CampaignName ?? "Sem campanha");
                AddInfo(table, "Projeto/destinacao", receipt.ProjectName ?? "Sem projeto/destinacao");
            });
        });
    }

    private static void AddInfo(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingBottom(8).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            column.Item().Text(value).FontSize(10).SemiBold();
        });
    }

    private static string Fit(string value, int maxLength) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 3), "...");

    private static string VerificationCode(Guid receiptId) =>
        receiptId.ToString("N")[..12].ToUpperInvariant();
}
