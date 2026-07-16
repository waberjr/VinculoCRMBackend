using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
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

        var page = await _context.LandingPages
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
                IsPublished = page.IsPublished,
                PublishedAtUtc = page.PublishedAtUtc,
                AppliedTemplateId = page.AppliedTemplateId,
                SubmissionLimitWindowMinutes = page.SubmissionLimitWindowMinutes,
                SubmissionLimitMaxAttempts = page.SubmissionLimitMaxAttempts,
                CustomFields = LandingPageContent.ParseFields(page.CustomFieldsJson),
                PublicUrl = LandingPageContent.PublicUrl(page.TargetType, page.TargetId),
                TrackableUrl = LandingPageContent.TrackableUrl(page.TargetType, page.TargetId),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return page;
    }
}
