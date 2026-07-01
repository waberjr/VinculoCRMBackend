using VinculoBackend.Application.ConfigurableOptions.Commands.CreateConfigurableOption;
using VinculoBackend.Application.ConfigurableOptions.Commands.DeleteConfigurableOption;
using VinculoBackend.Application.ConfigurableOptions.Commands.UpdateConfigurableOption;
using VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptionCategories;
using VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptions;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class ConfigurableOptions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetConfigurableOptions);
        groupBuilder.MapGet(GetConfigurableOptionCategories, "Categories");
        groupBuilder.MapPost(CreateConfigurableOption);
        groupBuilder.MapPut(UpdateConfigurableOption, "{id}");
        groupBuilder.MapDelete(DeleteConfigurableOption, "{id}");
    }

    public static async Task<Ok<IReadOnlyCollection<OptionCategoryDto>>> GetConfigurableOptionCategories(ISender sender)
    {
        var result = await sender.Send(new GetConfigurableOptionCategoriesQuery());
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyCollection<OptionDto>>> GetConfigurableOptions(
        ISender sender,
        ConfigurableOptionCategory? category,
        bool includeInactive = false)
    {
        var result = await sender.Send(new GetConfigurableOptionsQuery(category, includeInactive));
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateConfigurableOption(ISender sender, CreateConfigurableOptionCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/ConfigurableOptions/{id}", id);
    }

    public static async Task<NoContent> UpdateConfigurableOption(ISender sender, Guid id, UpdateConfigurableOptionCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteConfigurableOption(ISender sender, Guid id)
    {
        await sender.Send(new DeleteConfigurableOptionCommand(id));
        return TypedResults.NoContent();
    }
}
