using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class LandingPage : OrganizationEntity
{
    public string TargetType { get; private set; } = string.Empty;
    public Guid TargetId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Subtitle { get; private set; }
    public string? HeroImageUrl { get; private set; }
    public decimal? GoalAmount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsPublished { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public string? CustomFieldsJson { get; private set; }

    public static LandingPage Create(
        Guid organizationId,
        string targetType,
        Guid targetId,
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        bool isActive,
        bool isPublished = false,
        string? customFieldsJson = null,
        DateTimeOffset? publishedAtUtc = null)
    {
        var page = new LandingPage
        {
            OrganizationId = organizationId,
            TargetType = NormalizeTargetType(targetType),
            TargetId = targetId,
        };
        page.Update(title, subtitle, heroImageUrl, goalAmount, isActive, isPublished, customFieldsJson, publishedAtUtc);
        return page;
    }

    public void Update(
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        bool isActive,
        bool isPublished,
        string? customFieldsJson,
        DateTimeOffset? publishedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Informe o titulo da landing page.");
        }

        if (goalAmount is not null && goalAmount <= 0)
        {
            throw new DomainValidationException("A meta da landing page deve ser maior que zero.");
        }

        Title = title.Trim();
        Subtitle = TrimToNull(subtitle);
        HeroImageUrl = TrimToNull(heroImageUrl);
        GoalAmount = goalAmount;
        IsActive = isActive;
        IsPublished = isPublished;
        PublishedAtUtc = isPublished ? publishedAtUtc : null;
        CustomFieldsJson = TrimToNull(customFieldsJson);
    }

    private static string NormalizeTargetType(string targetType)
    {
        return targetType.Trim().ToLowerInvariant() switch
        {
            "campaign" => "campaign",
            "project" => "project",
            _ => throw new DomainValidationException("Tipo de landing page invalido."),
        };
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
