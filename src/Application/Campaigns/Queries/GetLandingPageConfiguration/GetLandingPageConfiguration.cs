using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageConfiguration;

public sealed record GetLandingPageConfigurationQuery(string TargetType, Guid TargetId) : IRequest<LandingPageConfigurationDto?>;

public sealed class GetLandingPageConfigurationQueryHandler : IRequestHandler<GetLandingPageConfigurationQuery, LandingPageConfigurationDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageConfigurationQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPageConfigurationDto?> Handle(GetLandingPageConfigurationQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var targetType = request.TargetType.Trim().ToLowerInvariant();

        return await _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == targetType && page.TargetId == request.TargetId)
            .Select(page => new LandingPageConfigurationDto
            {
                TargetType = page.TargetType,
                TargetId = page.TargetId,
                Title = page.Title,
                Subtitle = page.Subtitle,
                HeroImageUrl = page.HeroImageUrl,
                GoalAmount = page.GoalAmount,
                IsActive = page.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
