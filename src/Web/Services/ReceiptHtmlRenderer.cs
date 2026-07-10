using System.Globalization;
using System.Net;
using VinculoBackend.Application.Receipts.Models;

namespace VinculoBackend.Web.Services;

public sealed class ReceiptHtmlRenderer : IReceiptHtmlRenderer
{
    public string Render(ReceiptPrintDto receipt)
    {
        var organizationDocument = string.IsNullOrWhiteSpace(receipt.OrganizationDocument)
            ? string.Empty
            : $"<p>Documento: {WebUtility.HtmlEncode(receipt.OrganizationDocument)}</p>";
        var donorDocument = string.IsNullOrWhiteSpace(receipt.DonorDocument)
            ? string.Empty
            : $"<p>Documento do doador: {WebUtility.HtmlEncode(receipt.DonorDocument)}</p>";
        var campaign = string.IsNullOrWhiteSpace(receipt.CampaignName)
            ? "Sem campanha"
            : WebUtility.HtmlEncode(receipt.CampaignName);
        var project = string.IsNullOrWhiteSpace(receipt.ProjectName)
            ? "Sem projeto/destinacao"
            : WebUtility.HtmlEncode(receipt.ProjectName);
        var logo = string.IsNullOrWhiteSpace(receipt.OrganizationLogoUrl)
            ? $"<div class=\"logo-placeholder\">{WebUtility.HtmlEncode(receipt.OrganizationName[..Math.Min(2, receipt.OrganizationName.Length)].ToUpperInvariant())}</div>"
            : $"<img class=\"logo\" src=\"/api/Organizations/{receipt.OrganizationId}/Logo\" alt=\"Logo da organizacao\">";

        return $$"""
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <title>Recibo {{WebUtility.HtmlEncode(receipt.Number)}}</title>
  <style>
    * { box-sizing: border-box; }
    body { background: #f5f5f5; font-family: Arial, sans-serif; color: #171717; margin: 0; padding: 40px; }
    .receipt { max-width: 820px; margin: 0 auto; border: 1px solid #d4d4d4; border-radius: 8px; background: #fff; padding: 36px; }
    .header { display: flex; align-items: flex-start; justify-content: space-between; gap: 24px; border-bottom: 1px solid #e5e5e5; padding-bottom: 24px; }
    .logo { max-width: 150px; max-height: 72px; object-fit: contain; }
    .logo-placeholder { width: 72px; height: 72px; display: grid; place-items: center; border: 1px solid #d4d4d4; border-radius: 8px; color: #047857; font-weight: 700; }
    h1 { margin: 4px 0 0; font-size: 30px; }
    h2 { margin: 30px 0 10px; font-size: 14px; color: #047857; text-transform: uppercase; letter-spacing: .08em; }
    p { line-height: 1.55; margin: 6px 0; }
    .muted { color: #525252; font-size: 13px; }
    .amount { font-size: 28px; font-weight: 700; margin-top: 8px; }
    .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 18px 28px; }
    .statement { margin-top: 32px; border-top: 1px solid #e5e5e5; padding-top: 24px; }
    .verification { margin-top: 24px; border-radius: 8px; background: #f5f5f5; padding: 14px 16px; font-size: 13px; color: #404040; }
    @media print { body { background: #fff; padding: 0; } .receipt { border: none; border-radius: 0; } }
  </style>
</head>
<body>
  <main class="receipt">
    <section class="header">
      <div>
        <p class="muted">Recibo de doacao</p>
        <h1>{{WebUtility.HtmlEncode(receipt.Number)}}</h1>
        <p>Documento financeiro para registro e prestacao de contas.</p>
      </div>
      {{logo}}
    </section>
    <h2>Organizacao</h2>
    <p><strong>{{WebUtility.HtmlEncode(receipt.OrganizationName)}}</strong></p>
    {{organizationDocument}}
    <h2>Doador</h2>
    <p><strong>{{WebUtility.HtmlEncode(receipt.DonorName)}}</strong></p>
    {{donorDocument}}
    <h2>Contribuicao</h2>
    <div class="grid">
      <div><p class="muted">Valor</p><p class="amount">{{receipt.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}}</p></div>
      <div><p class="muted">Pagamento</p><p>{{receipt.PaidAtUtc:dd/MM/yyyy}}</p></div>
      <div><p class="muted">Campanha</p><p>{{campaign}}</p></div>
      <div><p class="muted">Projeto/destinacao</p><p>{{project}}</p></div>
      <div><p class="muted">Referencia</p><p>{{WebUtility.HtmlEncode(receipt.DonationReference)}}</p></div>
      <div><p class="muted">Emissao</p><p>{{receipt.IssuedAtUtc:dd/MM/yyyy HH:mm}}</p></div>
    </div>
    <section class="statement">
      <p>Declaramos o recebimento da contribuicao descrita acima.</p>
      <p class="muted">Este recibo foi gerado eletronicamente pelo Vinculo CRM Filantropico.</p>
    </section>
    <section class="verification">
      Codigo de verificacao: {{receipt.Id.ToString("N")[..12].ToUpperInvariant()}}<br>
      Identificador do recibo: {{receipt.Id}}
    </section>
  </main>
</body>
</html>
""";
    }
}
