using VinculoBackend.Application.Donations.Commands.CancelDonation;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donations.Commands.ConfirmDonation;
using VinculoBackend.Application.Donations.Commands.CreateDonation;
using VinculoBackend.Application.Donations.Commands.RefundDonation;
using VinculoBackend.Application.Donations.Models;
using VinculoBackend.Application.Donations.Queries.GetDonations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Donations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetDonations);
        groupBuilder.MapPost(CreateDonation);
        groupBuilder.MapPost(ConfirmDonation, "{id}/Confirm");
        groupBuilder.MapPost(CancelDonation, "{id}/Cancel");
        groupBuilder.MapPost(RefundDonation, "{id}/Refund");
    }

    public static async Task<Ok<PaginatedResult<DonationListItemDto>>> GetDonations(
        ISender sender,
        string? search,
        Guid? donorId,
        Guid? campaignId,
        string? status,
        string? paymentMethod,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetDonationsQuery
        {
            Search = search,
            DonorId = donorId,
            CampaignId = campaignId,
            Status = status,
            PaymentMethod = paymentMethod,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateDonation(ISender sender, CreateDonationCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Donations/{id}", id);
    }

    public static async Task<NoContent> ConfirmDonation(ISender sender, Guid id, ConfirmDonationCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelDonation(ISender sender, Guid id, CancelDonationCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> RefundDonation(ISender sender, Guid id, RefundDonationCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }
}
