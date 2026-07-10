using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Receipts.Commands.CancelReceipt;
using VinculoBackend.Application.Receipts.Commands.IssueReceipt;
using VinculoBackend.Application.Receipts.Commands.ReissueReceipt;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPdf;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;
using VinculoBackend.Application.Receipts.Queries.GetReceipts;
using VinculoBackend.Application.Receipts.Queries.ValidateReceipt;
using VinculoBackend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;

namespace VinculoBackend.Web.Endpoints;

public sealed class Receipts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetReceipts);
        groupBuilder.MapGet(ValidateReceipt, "{id}/Validate").AllowAnonymous();
        groupBuilder.MapGet(PrintReceipt, "{id}/Print");
        groupBuilder.MapGet(PdfReceipt, "{id}/Pdf");
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

    public static async Task<Results<Ok<ReceiptValidationDto>, NotFound>> ValidateReceipt(
        ISender sender,
        Guid id,
        string? code,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ValidateReceiptQuery(id, code), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
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

    public static async Task<Results<ContentHttpResult, NotFound>> PrintReceipt(
        ISender sender,
        IReceiptHtmlRenderer receiptHtmlRenderer,
        Guid id)
    {
        var receipt = await sender.Send(new GetReceiptPrintQuery(id));
        if (receipt is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Content(receiptHtmlRenderer.Render(receipt), "text/html", Encoding.UTF8);
    }

    public static async Task<Results<FileContentHttpResult, NotFound>> PdfReceipt(ISender sender, Guid id)
    {
        var receiptPdf = await sender.Send(new GetReceiptPdfQuery(id));
        if (receiptPdf is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.File(
            receiptPdf.Content,
            "application/pdf",
            receiptPdf.FileName);
    }

}
