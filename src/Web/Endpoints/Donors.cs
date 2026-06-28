using VinculoBackend.Application.Donors.Commands.AddDonorTimelineEntry;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Commands.CreateDonor;
using VinculoBackend.Application.Donors.Commands.UpdateDonor;
using VinculoBackend.Application.Donors.Models;
using VinculoBackend.Application.Donors.Queries.GetDonorById;
using VinculoBackend.Application.Donors.Queries.GetDonors;
using VinculoBackend.Application.Donors.Queries.GetDonorTimeline;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class Donors : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetDonors);
        groupBuilder.MapGet(GetDonorById, "{id}");
        groupBuilder.MapGet(GetDonorTimeline, "{id}/Timeline");
        groupBuilder.MapPost(AddTimelineEntry, "{id}/Timeline");
        groupBuilder.MapPost(CreateDonor);
        groupBuilder.MapPut(UpdateDonor, "{id}");
    }

    public static async Task<Ok<PaginatedResult<DonorListItemDto>>> GetDonors(
        ISender sender,
        string? search,
        Guid? statusOptionId,
        Guid? tagId,
        Guid? relationshipProfileOptionId,
        bool? allowsCommunication,
        bool? doNotContact,
        string? state,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetDonorsQuery
        {
            Search = search,
            StatusOptionId = statusOptionId,
            TagId = tagId,
            RelationshipProfileOptionId = relationshipProfileOptionId,
            AllowsCommunication = allowsCommunication,
            DoNotContact = doNotContact,
            State = state,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<DonorDetailDto>, NotFound>> GetDonorById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetDonorByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<DonorTimelineResponseDto>, NotFound>> GetDonorTimeline(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetDonorTimelineQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateDonor(ISender sender, CreateDonorCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Donors/{id}", id);
    }

    public static async Task<NoContent> UpdateDonor(ISender sender, Guid id, UpdateDonorCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<Created<Guid>> AddTimelineEntry(ISender sender, Guid id, AddDonorTimelineEntryCommand command)
    {
        var entryId = await sender.Send(command with { DonorId = id });
        return TypedResults.Created($"/api/Donors/{id}/Timeline/{entryId}", entryId);
    }
}
