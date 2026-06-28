using VinculoBackend.Application.Dashboard.Models;
using VinculoBackend.Application.Dashboard.Queries.GetDashboardOverview;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Dashboard : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetOverview, "Overview");
    }

    public static async Task<Ok<DashboardOverviewDto>> GetOverview(
        ISender sender,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc)
    {
        var result = await sender.Send(new GetDashboardOverviewQuery
        {
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
        });

        return TypedResults.Ok(result);
    }
}
