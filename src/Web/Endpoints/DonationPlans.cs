using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonationPlans.Models;
using VinculoBackend.Application.DonationPlans.Queries.GetDonationPlans;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class DonationPlans : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetDonationPlans);
    }

    public static async Task<Ok<PaginatedResult<DonationPlanListItemDto>>> GetDonationPlans(
        ISender sender,
        Guid? donorId,
        Guid? statusOptionId,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetDonationPlansQuery
        {
            DonorId = donorId,
            StatusOptionId = statusOptionId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }
}
