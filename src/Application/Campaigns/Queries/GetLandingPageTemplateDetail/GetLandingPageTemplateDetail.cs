using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageTemplateDetail;

public sealed record GetLandingPageTemplateDetailQuery(Guid Id) : IRequest<LandingPageTemplateDetailDto?>;

public sealed class GetLandingPageTemplateDetailQueryHandler : IRequestHandler<GetLandingPageTemplateDetailQuery, LandingPageTemplateDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageTemplateDetailQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPageTemplateDetailDto?> Handle(GetLandingPageTemplateDetailQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var template = await _context.LandingPageTemplates
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new LandingPageTemplateDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Category = entity.Category,
                Title = entity.Title,
                Subtitle = entity.Subtitle,
                HeroImageUrl = entity.HeroImageUrl,
                GoalAmount = entity.GoalAmount,
                IsActive = entity.IsActive,
                CustomFields = LandingPageContent.ParseFields(entity.CustomFieldsJson),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return null;
        }

        var pages = await _context.LandingPages
            .AsNoTracking()
            .Where(page => page.AppliedTemplateId == request.Id)
            .Select(page => new
            {
                page.TargetType,
                page.TargetId,
                page.IsActive,
                page.IsPublished,
                page.PublishedAtUtc,
                CampaignName = page.TargetType == "campaign"
                    ? _context.Campaigns.Where(campaign => campaign.Id == page.TargetId).Select(campaign => campaign.Name).FirstOrDefault()
                    : null,
                ProjectName = page.TargetType == "project"
                    ? _context.Projects.Where(project => project.Id == page.TargetId).Select(project => project.Name).FirstOrDefault()
                    : null,
            })
            .ToArrayAsync(cancellationToken);

        var auditEntries = await _context.LandingPageAuditEntries
            .AsNoTracking()
            .Where(entry => entry.EntityType == nameof(LandingPageTemplate) && entry.EntityId == request.Id)
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Take(20)
            .Select(entry => new LandingPageAuditEntryDto
            {
                Id = entry.Id,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                Title = entry.Title,
                Description = entry.Description,
                CreatedByUserId = entry.CreatedByUserId,
                OccurredAtUtc = entry.OccurredAtUtc,
            })
            .ToArrayAsync(cancellationToken);

        return new LandingPageTemplateDetailDto
        {
            Template = template,
            Uses = pages.Select(page => new LandingPageTemplateUsageDto
            {
                TargetType = page.TargetType,
                TargetId = page.TargetId,
                TargetName = page.CampaignName ?? page.ProjectName ?? $"{page.TargetType}/{page.TargetId}",
                IsActive = page.IsActive,
                IsPublished = page.IsPublished,
                PublishedAtUtc = page.PublishedAtUtc,
            }).ToArray(),
            AuditEntries = auditEntries,
        };
    }
}
