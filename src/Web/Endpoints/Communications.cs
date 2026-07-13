using VinculoBackend.Application.Communications.Commands.CancelCommunicationCampaign;
using Microsoft.AspNetCore.Http.HttpResults;
using VinculoBackend.Application.Communications.Commands.CreateCommunicationCampaign;
using VinculoBackend.Application.Communications.Commands.CreateCommunicationTemplate;
using VinculoBackend.Application.Communications.Commands.UpdateCommunicationCampaign;
using VinculoBackend.Application.Communications.Commands.UpdateCommunicationTemplate;
using VinculoBackend.Application.Communications.Models;
using VinculoBackend.Application.Communications.Queries.GetCommunicationCampaignRecipients;
using VinculoBackend.Application.Communications.Queries.GetCommunicationCampaigns;
using VinculoBackend.Application.Communications.Queries.GetCommunicationTemplates;

namespace VinculoBackend.Web.Endpoints;

public sealed class Communications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(Templates, "Templates");
        groupBuilder.MapPost(CreateTemplate, "Templates");
        groupBuilder.MapPut(UpdateTemplate, "Templates/{id}");
        groupBuilder.MapGet(Campaigns, "Campaigns");
        groupBuilder.MapGet(CampaignRecipients, "Campaigns/{id}/Recipients");
        groupBuilder.MapPost(CreateCampaign, "Campaigns");
        groupBuilder.MapPut(UpdateCampaign, "Campaigns/{id}");
        groupBuilder.MapPost(CancelCampaign, "Campaigns/{id}/Cancel");
    }

    public static async Task<Ok<IReadOnlyCollection<CommunicationTemplateDto>>> Templates(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCommunicationTemplatesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateTemplate(
        ISender sender,
        CreateCommunicationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/Communications/Templates/{id}", id);
    }

    public static async Task<NoContent> UpdateTemplate(
        ISender sender,
        Guid id,
        UpdateCommunicationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Ok<IReadOnlyCollection<CommunicationCampaignDto>>> Campaigns(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCommunicationCampaignsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<IReadOnlyCollection<CommunicationRecipientDto>>, NotFound>> CampaignRecipients(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCommunicationCampaignRecipientsQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateCampaign(
        ISender sender,
        CreateCommunicationCampaignCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/Communications/Campaigns/{id}", id);
    }

    public static async Task<NoContent> UpdateCampaign(
        ISender sender,
        Guid id,
        UpdateCommunicationCampaignCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelCampaign(
        ISender sender,
        Guid id,
        CancelCommunicationCampaignCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return TypedResults.NoContent();
    }
}
