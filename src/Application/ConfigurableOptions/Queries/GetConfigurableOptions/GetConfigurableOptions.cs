using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ConfigurableOptions.Queries.GetConfigurableOptions;

public record GetConfigurableOptionsQuery(ConfigurableOptionCategory? Category = null, bool IncludeInactive = false) : IRequest<IReadOnlyCollection<OptionDto>>;

public sealed class GetConfigurableOptionsQueryHandler : IRequestHandler<GetConfigurableOptionsQuery, IReadOnlyCollection<OptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetConfigurableOptionsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OptionDto>> Handle(GetConfigurableOptionsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.ConfigurableOptions.AsNoTracking();

        if (request.Category is not null)
        {
            var category = request.Category.Value.ToString();
            query = query.Where(option => option.Category == category);
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(option => option.IsActive);
        }

        return await query
            .OrderBy(option => option.Category)
            .ThenBy(option => option.SortOrder)
            .ThenBy(option => option.Name)
            .Select(option => new OptionDto
            {
                Id = option.Id,
                Category = option.Category,
                Code = option.Code,
                Name = option.Name,
                Description = option.Description,
                Color = option.Color,
                SortOrder = option.SortOrder,
                IsSystem = option.IsSystem,
                IsActive = option.IsActive,
            })
            .ToListAsync(cancellationToken);
    }
}
