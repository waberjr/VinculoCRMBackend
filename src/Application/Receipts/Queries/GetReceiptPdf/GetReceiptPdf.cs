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
            : new ReceiptPdfDto($"{receipt.Number}.pdf", CreateReceiptPdf(receipt));
    }

    private static byte[] CreateReceiptPdf(ReceiptPrintDto receipt)
    {
        var content = new StringBuilder();
        DrawRectangle(content, 36, 54, 523, 734);
        DrawLine(content, 36, 705, 559, 705);
        DrawLine(content, 36, 610, 559, 610);
        DrawLine(content, 36, 480, 559, 480);
        DrawLine(content, 36, 188, 559, 188);
        DrawRectangle(content, 438, 724, 86, 44);
        DrawVerificationQr(content, receipt.Id, 452, 210, 72);

        AddText(content, "RECIBO DE DOACAO", 20, 58, 755);
        AddText(content, receipt.Number, 13, 58, 732);
        AddText(content, "Documento financeiro para registro e prestacao de contas.", 10, 58, 714);
        AddText(content, string.IsNullOrWhiteSpace(receipt.OrganizationLogoUrl) ? "Vinculo" : "Logo configurado", 10, 452, 748);
        AddText(content, "CRM Filantropico", 7, 452, 734);

        AddText(content, "ORGANIZACAO", 10, 58, 680);
        AddText(content, Fit(receipt.OrganizationName, 74), 13, 58, 660);
        AddText(content, Fit(string.IsNullOrWhiteSpace(receipt.OrganizationDocument) ? "Documento nao informado" : $"Documento: {receipt.OrganizationDocument}", 86), 10, 58, 642);

        AddText(content, "DOADOR", 10, 58, 585);
        AddText(content, Fit(receipt.DonorName, 74), 13, 58, 565);
        AddText(content, Fit(string.IsNullOrWhiteSpace(receipt.DonorDocument) ? "Documento nao informado" : $"Documento: {receipt.DonorDocument}", 86), 10, 58, 547);

        AddText(content, "VALOR RECEBIDO", 10, 58, 450);
        AddText(content, receipt.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), 24, 58, 420);
        AddText(content, $"Pagamento: {receipt.PaidAtUtc:dd/MM/yyyy}", 11, 58, 388);
        AddText(content, $"Emissao: {receipt.IssuedAtUtc:dd/MM/yyyy HH:mm}", 11, 58, 368);
        AddText(content, Fit($"Referencia: {receipt.DonationReference}", 78), 11, 58, 348);
        AddText(content, Fit($"Campanha: {receipt.CampaignName ?? "Sem campanha"}", 78), 11, 58, 328);
        AddText(content, Fit($"Projeto/destinacao: {receipt.ProjectName ?? "Sem projeto/destinacao"}", 78), 11, 58, 308);

        AddText(content, "Declaramos o recebimento da contribuicao descrita acima.", 11, 58, 248);
        AddText(content, $"Codigo de verificacao: {VerificationCode(receipt.Id)}", 10, 58, 222);
        AddText(content, "Confira este codigo com a organizacao emissora em caso de duvida.", 9, 58, 204);
        AddText(content, "Este documento foi gerado eletronicamente pelo Vinculo CRM Filantropico.", 10, 58, 168);
        AddText(content, $"Identificador do recibo: {receipt.Id}", 9, 58, 148);

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

    private static void AddText(StringBuilder content, string value, int size, int x, int y)
    {
        content.AppendLine("BT");
        content.AppendLine($"/F1 {size} Tf");
        content.AppendLine($"{x} {y} Td");
        content.Append('(').Append(EscapePdf(value)).AppendLine(") Tj");
        content.AppendLine("ET");
    }

    private static void DrawLine(StringBuilder content, int x1, int y1, int x2, int y2)
    {
        content.AppendLine($"{x1} {y1} m");
        content.AppendLine($"{x2} {y2} l");
        content.AppendLine("S");
    }

    private static void DrawRectangle(StringBuilder content, int x, int y, int width, int height)
    {
        content.AppendLine($"{x} {y} {width} {height} re");
        content.AppendLine("S");
    }

    private static void FillRectangle(StringBuilder content, int x, int y, int width, int height)
    {
        content.AppendLine($"{x} {y} {width} {height} re");
        content.AppendLine("f");
    }

    private static void DrawVerificationQr(StringBuilder content, Guid receiptId, int x, int y, int size)
    {
        const int modules = 21;
        var moduleSize = Math.Max(1, size / modules);
        var seed = receiptId.ToByteArray();

        for (var row = 0; row < modules; row++)
        {
            for (var column = 0; column < modules; column++)
            {
                if (IsFinder(row, column) || ShouldFillModule(seed, row, column))
                {
                    FillRectangle(content, x + column * moduleSize, y + (modules - row - 1) * moduleSize, moduleSize, moduleSize);
                }
            }
        }
    }

    private static bool IsFinder(int row, int column)
    {
        return IsFinderAt(row, column, 0, 0) ||
            IsFinderAt(row, column, 0, 14) ||
            IsFinderAt(row, column, 14, 0);
    }

    private static bool IsFinderAt(int row, int column, int startRow, int startColumn)
    {
        var inside = row >= startRow && row < startRow + 7 && column >= startColumn && column < startColumn + 7;
        if (!inside)
        {
            return false;
        }

        var localRow = row - startRow;
        var localColumn = column - startColumn;
        return localRow is 0 or 6 || localColumn is 0 or 6 || (localRow is >= 2 and <= 4 && localColumn is >= 2 and <= 4);
    }

    private static bool ShouldFillModule(byte[] seed, int row, int column)
    {
        var index = (row * 21 + column) % seed.Length;
        return ((seed[index] + row * 17 + column * 31) & 3) == 0;
    }

    private static string EscapePdf(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);

    private static string Fit(string value, int maxLength) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 3), "...");

    private static string VerificationCode(Guid receiptId) =>
        receiptId.ToString("N")[..12].ToUpperInvariant();
}
