using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VinculoBackend.Web.Endpoints;

public sealed class SystemOptions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetSystemOptions);
    }

    public static Results<Ok<IReadOnlyCollection<OptionDto>>, BadRequest<string>> GetSystemOptions(string category)
    {
        var result = category switch
        {
            nameof(DonorPersonType) => SystemOptionMapper.ToOptionDtos<DonorPersonType>(),
            nameof(DonorStatus) => SystemOptionMapper.ToOptionDtos<DonorStatus>(),
            nameof(DonationType) => SystemOptionMapper.ToOptionDtos<DonationType>(),
            nameof(DonationStatus) => SystemOptionMapper.ToOptionDtos<DonationStatus>(),
            nameof(PaymentMethod) => SystemOptionMapper.ToOptionDtos<PaymentMethod>(),
            nameof(DonationPlanStatus) => SystemOptionMapper.ToOptionDtos<DonationPlanStatus>(),
            nameof(CampaignType) => SystemOptionMapper.ToOptionDtos<CampaignType>(),
            nameof(CampaignStatus) => SystemOptionMapper.ToOptionDtos<CampaignStatus>(),
            nameof(ProjectStatus) => SystemOptionMapper.ToOptionDtos<ProjectStatus>(),
            nameof(CampaignChannel) => SystemOptionMapper.ToOptionDtos<CampaignChannel>(),
            nameof(TaskType) => SystemOptionMapper.ToOptionDtos<TaskType>(),
            nameof(TaskPriority) => SystemOptionMapper.ToOptionDtos<TaskPriority>(),
            nameof(RelationshipTaskStatus) => SystemOptionMapper.ToOptionDtos<RelationshipTaskStatus>(),
            nameof(ContactOutcome) => SystemOptionMapper.ToOptionDtos<ContactOutcome>(),
            nameof(TimelineEntryType) => SystemOptionMapper.ToOptionDtos<TimelineEntryType>(),
            nameof(PhoneType) => SystemOptionMapper.ToOptionDtos<PhoneType>(),
            nameof(EmailType) => SystemOptionMapper.ToOptionDtos<EmailType>(),
            _ => null,
        };

        return result is null
            ? TypedResults.BadRequest("Categoria de opção fixa inválida.")
            : TypedResults.Ok(result);
    }
}
