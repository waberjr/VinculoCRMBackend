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
    public Guid? AppliedTemplateId { get; private set; }
    public string? CustomFieldsJson { get; private set; }
    public int SubmissionLimitWindowMinutes { get; private set; } = 15;
    public int SubmissionLimitMaxAttempts { get; private set; } = 5;

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
        DateTimeOffset? publishedAtUtc = null,
        Guid? appliedTemplateId = null,
        int submissionLimitWindowMinutes = 15,
        int submissionLimitMaxAttempts = 5)
    {
        var page = new LandingPage
        {
            OrganizationId = organizationId,
            TargetType = NormalizeTargetType(targetType),
            TargetId = targetId,
        };
        page.Update(title, subtitle, heroImageUrl, goalAmount, isActive, isPublished, customFieldsJson, publishedAtUtc, appliedTemplateId, submissionLimitWindowMinutes, submissionLimitMaxAttempts);
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
        DateTimeOffset? publishedAtUtc,
        Guid? appliedTemplateId = null,
        int submissionLimitWindowMinutes = 15,
        int submissionLimitMaxAttempts = 5)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Informe o titulo da landing page.");
        }

        if (goalAmount is not null && goalAmount <= 0)
        {
            throw new DomainValidationException("A meta da landing page deve ser maior que zero.");
        }

        if (submissionLimitWindowMinutes is < 1 or > 1440)
        {
            throw new DomainValidationException("A janela de bloqueio deve ter entre 1 e 1440 minutos.");
        }

        if (submissionLimitMaxAttempts is < 1 or > 100)
        {
            throw new DomainValidationException("O limite de tentativas deve estar entre 1 e 100.");
        }

        Title = title.Trim();
        Subtitle = TrimToNull(subtitle);
        HeroImageUrl = TrimToNull(heroImageUrl);
        GoalAmount = goalAmount;
        IsActive = isActive;
        IsPublished = isPublished;
        PublishedAtUtc = isPublished ? publishedAtUtc : null;
        AppliedTemplateId = appliedTemplateId;
        CustomFieldsJson = TrimToNull(customFieldsJson);
        SubmissionLimitWindowMinutes = submissionLimitWindowMinutes;
        SubmissionLimitMaxAttempts = submissionLimitMaxAttempts;
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
