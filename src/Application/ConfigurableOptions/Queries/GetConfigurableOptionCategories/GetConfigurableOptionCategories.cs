using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptionCategories;

public record GetConfigurableOptionCategoriesQuery : IRequest<IReadOnlyCollection<OptionCategoryDto>>;

public sealed class GetConfigurableOptionCategoriesQueryHandler : IRequestHandler<GetConfigurableOptionCategoriesQuery, IReadOnlyCollection<OptionCategoryDto>>
{
    private static readonly IReadOnlyCollection<OptionCategoryDto> EditableCategories =
    [
        Category(ConfigurableOptionCategory.RelationshipProfile, "Perfil de relacionamento", 30),
        Category(ConfigurableOptionCategory.DonorSource, "Origem do doador", 40),
        Category(ConfigurableOptionCategory.ContactChannel, "Canal de contato", 50),
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
