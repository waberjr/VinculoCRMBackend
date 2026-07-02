using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonationPlans.Commands.CancelDonationPlan;
using VinculoBackend.Application.DonationPlans.Commands.CreateDonationPlan;
using VinculoBackend.Application.DonationPlans.Commands.PauseDonationPlan;
using VinculoBackend.Application.DonationPlans.Commands.ResumeDonationPlan;
using VinculoBackend.Application.DonationPlans.Commands.UpdateDonationPlan;
using VinculoBackend.Application.DonationPlans.Models;
using VinculoBackend.Application.DonationPlans.Queries.GetDonationPlanById;
using VinculoBackend.Application.DonationPlans.Queries.GetDonationPlans;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class DonationPlans : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetDonationPlans);
        groupBuilder.MapGet(GetDonationPlanById, "{id}");
        groupBuilder.MapPost(CreateDonationPlan);
        groupBuilder.MapPut(UpdateDonationPlan, "{id}");
        groupBuilder.MapPost(PauseDonationPlan, "{id}/Pause");
        groupBuilder.MapPost(ResumeDonationPlan, "{id}/Resume");
        groupBuilder.MapPost(CancelDonationPlan, "{id}/Cancel");
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

    public static async Task<Results<Ok<DonationPlanListItemDto>, NotFound>> GetDonationPlanById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetDonationPlanByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<NoContent> UpdateDonationPlan(ISender sender, Guid id, UpdateDonationPlanCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> PauseDonationPlan(ISender sender, Guid id)
    {
        await sender.Send(new PauseDonationPlanCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ResumeDonationPlan(ISender sender, Guid id)
    {
        await sender.Send(new ResumeDonationPlanCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelDonationPlan(ISender sender, Guid id, CancelDonationPlanCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }
}
