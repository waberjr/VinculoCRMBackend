using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonationPlans.Commands.CreateDonationPlan;
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
        groupBuilder.MapPost(CreateDonationPlan);
    }

    public static async Task<Ok<PaginatedResult<DonationPlanListItemDto>>> GetDonationPlans(
        ISender sender,
        Guid? donorId,
        string? status,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetDonationPlansQuery
        {
            DonorId = donorId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateDonationPlan(ISender sender, CreateDonationPlanCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/DonationPlans/{id}", id);
    }
}
