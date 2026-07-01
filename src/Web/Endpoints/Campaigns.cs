using VinculoBackend.Application.Campaigns.Commands.ActivateCampaign;
using VinculoBackend.Application.Campaigns.Commands.CompleteCampaign;
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
        groupBuilder.MapPost(ActivateCampaign, "{id}/Activate");
        groupBuilder.MapPost(CompleteCampaign, "{id}/Complete");
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

    public static async Task<NoContent> ActivateCampaign(ISender sender, Guid id)
    {
        await sender.Send(new ActivateCampaignCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CompleteCampaign(ISender sender, Guid id)
    {
        await sender.Send(new CompleteCampaignCommand(id));
        return TypedResults.NoContent();
    }
}
