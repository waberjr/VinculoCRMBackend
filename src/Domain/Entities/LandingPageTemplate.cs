using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class LandingPageTemplate : OrganizationEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Subtitle { get; private set; }
    public string? HeroImageUrl { get; private set; }
    public decimal? GoalAmount { get; private set; }
    public string? CustomFieldsJson { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; } = 1;

    public static LandingPageTemplate Create(
        Guid organizationId,
        string name,
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        string? customFieldsJson,
        string? category = null)
    {
        var template = new LandingPageTemplate { OrganizationId = organizationId };
        template.Update(name, title, subtitle, heroImageUrl, goalAmount, customFieldsJson, true, category, incrementVersion: false);
        return template;
    }

    public void Update(
        string name,
        string title,
        string? subtitle,
        string? heroImageUrl,
        decimal? goalAmount,
        string? customFieldsJson,
        bool isActive,
        string? category = null,
        bool incrementVersion = true)
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
        Category = TrimToNull(category);
        Title = title.Trim();
        Subtitle = TrimToNull(subtitle);
        HeroImageUrl = TrimToNull(heroImageUrl);
        GoalAmount = goalAmount;
        CustomFieldsJson = TrimToNull(customFieldsJson);
        IsActive = isActive;
        if (incrementVersion)
        {
            Version++;
        }
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
