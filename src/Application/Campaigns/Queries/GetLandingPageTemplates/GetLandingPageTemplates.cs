using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageTemplates;

public sealed record GetLandingPageTemplatesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyCollection<LandingPageTemplateDto>>;

public sealed class GetLandingPageTemplatesQueryHandler : IRequestHandler<GetLandingPageTemplatesQuery, IReadOnlyCollection<LandingPageTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageTemplatesQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<LandingPageTemplateDto>> Handle(GetLandingPageTemplatesQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.LandingPageTemplates.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(template => template.IsActive);
        }

        return await query
            .OrderBy(template => template.Name)
            .Select(template => new LandingPageTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Category = template.Category,
                Title = template.Title,
                Subtitle = template.Subtitle,
                HeroImageUrl = template.HeroImageUrl,
                GoalAmount = template.GoalAmount,
                IsActive = template.IsActive,
                Version = template.Version,
                CustomFields = LandingPageContent.ParseFields(template.CustomFieldsJson),
            })
            .ToArrayAsync(cancellationToken);
    }
}
