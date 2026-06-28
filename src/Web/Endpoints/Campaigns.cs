using VinculoBackend.Application.Campaigns.Commands.CreateCampaign;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetCampaigns;
using VinculoBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Campaigns : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetCampaigns);
        groupBuilder.MapPost(CreateCampaign);
    }

    public static async Task<Ok<PaginatedResult<CampaignListItemDto>>> GetCampaigns(
        ISender sender,
        string? search,
        Guid? statusOptionId,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetCampaignsQuery
        {
            Search = search,
            StatusOptionId = statusOptionId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateCampaign(ISender sender, CreateCampaignCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Campaigns/{id}", id);
    }
}
