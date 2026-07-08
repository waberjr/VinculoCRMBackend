using System.Text;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;

namespace VinculoBackend.Application.Receipts.Queries.GetReceiptPdf;

public sealed record GetReceiptPdfQuery(Guid Id) : IRequest<ReceiptPdfDto?>;

public sealed class GetReceiptPdfQueryHandler : IRequestHandler<GetReceiptPdfQuery, ReceiptPdfDto?>
{
    private readonly ISender _sender;

    public GetReceiptPdfQueryHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<ReceiptPdfDto?> Handle(GetReceiptPdfQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _sender.Send(new GetReceiptPrintQuery(request.Id), cancellationToken);
        return receipt is null
            ? null
            : new ReceiptPdfDto($"{receipt.Number}.pdf", CreateSimplePdf(ReceiptLines(receipt)));
    }

    private static IReadOnlyCollection<string> ReceiptLines(ReceiptPrintDto receipt)
    {
        return
        [
            $"RECIBO DE DOACAO {receipt.Number}",
            "Declaramos o recebimento da contribuicao abaixo para fins de registro e prestacao de contas.",
            " ",
            $"Organizacao: {receipt.OrganizationName}",
            string.IsNullOrWhiteSpace(receipt.OrganizationDocument) ? "" : $"Documento organizacao: {receipt.OrganizationDocument}",
            " ",
            $"Doador: {receipt.DonorName}",
            string.IsNullOrWhiteSpace(receipt.DonorDocument) ? "" : $"Documento doador: {receipt.DonorDocument}",
            " ",
            $"Valor: {receipt.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))}",
            $"Pagamento: {receipt.PaidAtUtc:dd/MM/yyyy}",
            $"Campanha: {receipt.CampaignName ?? "Sem campanha"}",
            $"Projeto/destinacao: {receipt.ProjectName ?? "Sem projeto/destinacao"}",
            $"Referencia: {receipt.DonationReference}",
            " ",
            $"Emitido em: {receipt.IssuedAtUtc:dd/MM/yyyy HH:mm}",
            "Documento gerado pelo Vinculo CRM Filantropico.",
        ];
    }

    private static byte[] CreateSimplePdf(IReadOnlyCollection<string> lines)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 12 Tf");
        content.AppendLine("50 780 Td");
        foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            content.Append('(').Append(EscapePdf(line)).AppendLine(") Tj");
            content.AppendLine("0 -22 Td");
        }
        content.AppendLine("ET");

        var stream = content.ToString();
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream",
        };

        var pdf = new StringBuilder();
        var offsets = new List<int> { 0 };
        pdf.AppendLine("%PDF-1.4");
        foreach (var (obj, index) in objects.Select((obj, index) => (obj, index)))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.AppendLine($"{index + 1} 0 obj");
            pdf.AppendLine(obj);
            pdf.AppendLine("endobj");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.AppendLine("xref");
        pdf.AppendLine($"0 {objects.Length + 1}");
        pdf.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1))
        {
            pdf.AppendLine($"{offset:0000000000} 00000 n ");
        }
        pdf.AppendLine("trailer");
        pdf.AppendLine($"<< /Size {objects.Length + 1} /Root 1 0 R >>");
        pdf.AppendLine("startxref");
        pdf.AppendLine(xrefOffset.ToString(System.Globalization.CultureInfo.InvariantCulture));
        pdf.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscapePdf(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
