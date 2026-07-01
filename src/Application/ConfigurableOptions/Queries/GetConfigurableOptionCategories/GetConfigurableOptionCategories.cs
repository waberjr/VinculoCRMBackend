using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptionCategories;

public record GetConfigurableOptionCategoriesQuery : IRequest<IReadOnlyCollection<OptionCategoryDto>>;

public sealed class GetConfigurableOptionCategoriesQueryHandler : IRequestHandler<GetConfigurableOptionCategoriesQuery, IReadOnlyCollection<OptionCategoryDto>>
{
    private static readonly IReadOnlyCollection<OptionCategoryDto> EditableCategories =
    [
        Category(ConfigurableOptionCategory.DonorPersonType, "Tipo de pessoa", 10),
        Category(ConfigurableOptionCategory.DonorStatus, "Status de doador", 20),
        Category(ConfigurableOptionCategory.RelationshipProfile, "Perfil de relacionamento", 30),
        Category(ConfigurableOptionCategory.DonorSource, "Origem do doador", 40),
        Category(ConfigurableOptionCategory.ContactChannel, "Canal de contato", 50),
        Category(ConfigurableOptionCategory.PhoneType, "Tipo de telefone", 60),
        Category(ConfigurableOptionCategory.EmailType, "Tipo de e-mail", 70),
        Category(ConfigurableOptionCategory.DonationType, "Tipo de contribuicao", 80),
        Category(ConfigurableOptionCategory.DonationStatus, "Status de contribuicao", 90),
        Category(ConfigurableOptionCategory.PaymentMethod, "Forma de pagamento", 100),
        Category(ConfigurableOptionCategory.DonationPlanStatus, "Status de recorrencia", 110),
        Category(ConfigurableOptionCategory.CampaignType, "Tipo de campanha", 120),
        Category(ConfigurableOptionCategory.CampaignStatus, "Status de campanha", 130),
        Category(ConfigurableOptionCategory.CampaignChannel, "Canal de campanha", 140),
        Category(ConfigurableOptionCategory.TaskType, "Tipo de tarefa", 150),
        Category(ConfigurableOptionCategory.TaskPriority, "Prioridade de tarefa", 160),
        Category(ConfigurableOptionCategory.TaskStatus, "Status de tarefa", 170),
        Category(ConfigurableOptionCategory.ContactOutcome, "Resultado de contato", 180),
        Category(ConfigurableOptionCategory.TimelineType, "Tipo de historico", 190),
    ];

    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetConfigurableOptionCategoriesQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OptionCategoryDto>> Handle(GetConfigurableOptionCategoriesQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var existingCategories = await _context.ConfigurableOptions
            .AsNoTracking()
            .Select(option => option.Category)
            .Distinct()
            .ToListAsync(cancellationToken);

        return EditableCategories
            .Where(category => existingCategories.Contains(category.Code))
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToList();
    }

    private static OptionCategoryDto Category(ConfigurableOptionCategory category, string name, int sortOrder)
    {
        return new OptionCategoryDto
        {
            Code = category.ToString(),
            Name = name,
            SortOrder = sortOrder,
        };
    }
}
