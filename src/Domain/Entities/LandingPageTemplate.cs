using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class LandingPageTemplate : OrganizationEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Subtitle { get; private set; }
    public string? HeroImageUrl { get; private set; }
    public decimal? GoalAmount { get; private set; }
    public string? CustomFieldsJson { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static LandingPageTemplate Create(
        Guid organizationId,
        string name,
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        string? customFieldsJson)
    {
        var template = new LandingPageTemplate { OrganizationId = organizationId };
        template.Update(name, title, subtitle, heroImageUrl, goalAmount, customFieldsJson, true);
        return template;
    }

    public void Update(
        string name,
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        string? customFieldsJson,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Informe o nome do template.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Informe o titulo do template.");
        }

        if (goalAmount is not null && goalAmount <= 0)
        {
            throw new DomainValidationException("A meta do template deve ser maior que zero.");
        }

        Name = name.Trim();
        Title = title.Trim();
        Subtitle = TrimToNull(subtitle);
        HeroImageUrl = TrimToNull(heroImageUrl);
        GoalAmount = goalAmount;
        CustomFieldsJson = TrimToNull(customFieldsJson);
        IsActive = isActive;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
