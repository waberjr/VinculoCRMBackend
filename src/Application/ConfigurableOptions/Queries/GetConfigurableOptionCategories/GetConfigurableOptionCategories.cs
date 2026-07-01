using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptionCategories;

public record GetConfigurableOptionCategoriesQuery : IRequest<IReadOnlyCollection<OptionCategoryDto>>;

public sealed class GetConfigurableOptionCategoriesQueryHandler : IRequestHandler<GetConfigurableOptionCategoriesQuery, IReadOnlyCollection<OptionCategoryDto>>
{
    private static readonly IReadOnlyCollection<OptionCategoryDto> EditableCategories =
    [
        Category("DonorPersonType", "Tipo de pessoa", 10),
        Category("DonorStatus", "Status de doador", 20),
        Category("RelationshipProfile", "Perfil de relacionamento", 30),
        Category("DonorSource", "Origem do doador", 40),
        Category("ContactChannel", "Canal de contato", 50),
        Category("PhoneType", "Tipo de telefone", 60),
        Category("EmailType", "Tipo de e-mail", 70),
        Category("DonationType", "Tipo de contribuicao", 80),
        Category("DonationStatus", "Status de contribuicao", 90),
        Category("PaymentMethod", "Forma de pagamento", 100),
        Category("DonationPlanStatus", "Status de recorrencia", 110),
        Category("CampaignType", "Tipo de campanha", 120),
        Category("CampaignStatus", "Status de campanha", 130),
        Category("CampaignChannel", "Canal de campanha", 140),
        Category("TaskType", "Tipo de tarefa", 150),
        Category("TaskPriority", "Prioridade de tarefa", 160),
        Category("TaskStatus", "Status de tarefa", 170),
        Category("ContactOutcome", "Resultado de contato", 180),
        Category("TimelineType", "Tipo de historico", 190),
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

    private static OptionCategoryDto Category(string code, string name, int sortOrder)
    {
        return new OptionCategoryDto
        {
            Code = code,
            Name = name,
            SortOrder = sortOrder,
        };
    }
}
