using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Services;

public static class LandingPageTemplateSnapshots
{
    public static LandingPageTemplateVersion FromTemplate(LandingPageTemplate template, DateTimeOffset createdAtUtc, string? userId)
    {
        return new LandingPageTemplateVersion
        {
            OrganizationId = template.OrganizationId,
            TemplateId = template.Id,
            Version = template.Version,
            Name = template.Name,
            Category = template.Category,
            Title = template.Title,
            Subtitle = template.Subtitle,
            HeroImageUrl = template.HeroImageUrl,
            GoalAmount = template.GoalAmount,
            CustomFieldsJson = template.CustomFieldsJson,
            IsActive = template.IsActive,
            CreatedAtUtc = createdAtUtc,
            CreatedByUserId = userId,
        };
    }
}
