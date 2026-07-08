using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Receipts.Commands.IssueReceipt;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Application.Receipts.Queries.GetReceipts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Receipts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetReceipts);
        groupBuilder.MapPost(IssueReceipt, "Issue");
    }

    public static async Task<Ok<PaginatedResult<ReceiptListItemDto>>> GetReceipts(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(new GetReceiptsQuery
        {
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
}
