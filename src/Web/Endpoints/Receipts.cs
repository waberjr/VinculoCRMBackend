using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Receipts.Commands.CancelReceipt;
using VinculoBackend.Application.Receipts.Commands.IssueReceipt;
using VinculoBackend.Application.Receipts.Commands.ReissueReceipt;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;
using VinculoBackend.Application.Receipts.Queries.GetReceipts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Text;

namespace VinculoBackend.Web.Endpoints;

public sealed class Receipts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetReceipts);
        groupBuilder.MapGet(PrintReceipt, "{id}/Print");
        groupBuilder.MapPost(IssueReceipt, "Issue");
        groupBuilder.MapPost(CancelReceipt, "{id}/Cancel");
        groupBuilder.MapPost(ReissueReceipt, "{id}/Reissue");
    }

    public static async Task<Ok<PaginatedResult<ReceiptListItemDto>>> GetReceipts(
        ISender sender,
        Guid? donorId,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(new GetReceiptsQuery
        {
            DonorId = donorId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> IssueReceipt(ISender sender, IssueReceiptCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Receipts/{id}", id);
    }

    public static async Task<NoContent> CancelReceipt(ISender sender, Guid id, CancelReceiptCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ReissueReceipt(ISender sender, Guid id, ReissueReceiptCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<Results<ContentHttpResult, NotFound>> PrintReceipt(ISender sender, Guid id)
    {
        var receipt = await sender.Send(new GetReceiptPrintQuery(id));
        if (receipt is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Content(RenderReceiptHtml(receipt), "text/html", Encoding.UTF8);
    }

    private static string RenderReceiptHtml(ReceiptPrintDto receipt)
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

        return $$"""
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <title>Recibo {{WebUtility.HtmlEncode(receipt.Number)}}</title>
  <style>
    body { font-family: Arial, sans-serif; color: #171717; margin: 40px; }
    .receipt { max-width: 760px; margin: 0 auto; border: 1px solid #d4d4d4; padding: 32px; }
    h1 { margin: 0; font-size: 28px; }
    h2 { margin-top: 28px; font-size: 16px; color: #047857; }
    p { line-height: 1.55; }
    .muted { color: #525252; font-size: 13px; }
    .amount { font-size: 24px; font-weight: 700; margin-top: 8px; }
    .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    @media print { body { margin: 0; } .receipt { border: none; } }
  </style>
</head>
<body>
  <main class="receipt">
    <p class="muted">Recibo de doacao</p>
    <h1>{{WebUtility.HtmlEncode(receipt.Number)}}</h1>
    <p>Declaramos o recebimento da contribuicao abaixo para fins de registro e prestacao de contas.</p>
    <h2>Organizacao</h2>
    <p><strong>{{WebUtility.HtmlEncode(receipt.OrganizationName)}}</strong></p>
    {{organizationDocument}}
    <h2>Doador</h2>
    <p><strong>{{WebUtility.HtmlEncode(receipt.DonorName)}}</strong></p>
    {{donorDocument}}
    <h2>Contribuicao</h2>
    <div class="grid">
      <div><p class="muted">Valor</p><p class="amount">{{receipt.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))}}</p></div>
      <div><p class="muted">Pagamento</p><p>{{receipt.PaidAtUtc:dd/MM/yyyy}}</p></div>
      <div><p class="muted">Campanha</p><p>{{campaign}}</p></div>
      <div><p class="muted">Projeto/destinacao</p><p>{{project}}</p></div>
      <div><p class="muted">Referencia</p><p>{{WebUtility.HtmlEncode(receipt.DonationReference)}}</p></div>
      <div><p class="muted">Emissao</p><p>{{receipt.IssuedAtUtc:dd/MM/yyyy HH:mm}}</p></div>
    </div>
  </main>
</body>
</html>
""";
    }
}
