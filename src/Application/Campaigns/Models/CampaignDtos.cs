namespace VinculoBackend.Application.Campaigns.Models;

public sealed class CampaignListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonorsCount { get; init; }
    public int DonationsCount { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string AssignedUserName { get; init; } = "Sem responsavel";
}

public sealed class CampaignReportDto
{
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public decimal GoalAmount { get; init; }
    public int CampaignsCount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
    public decimal AverageDonationAmount { get; init; }
    public IReadOnlyCollection<CampaignReportItemDto> Campaigns { get; init; } = [];
    public IReadOnlyCollection<CampaignReportPeriodDto> Periods { get; init; } = [];
}

public sealed class CampaignReportItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
    public decimal AverageDonationAmount { get; init; }
    public decimal GoalPercentage { get; init; }
}

public sealed class CampaignReportPeriodDto
{
    public string Period { get; init; } = string.Empty;
    public decimal ConfirmedAmount { get; init; }
    public int DonationsCount { get; init; }
    public int DonorsCount { get; init; }
}

public sealed class PublicLandingPageDto
{
    public string Kind { get; init; } = string.Empty;
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal GoalAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int DonorsCount { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields { get; init; } = [];
}

public sealed class LandingPageConfigurationDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal? GoalAmount { get; init; }
    public bool IsActive { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAtUtc { get; init; }
    public Guid? AppliedTemplateId { get; init; }
    public int SubmissionLimitWindowMinutes { get; init; } = 15;
    public int SubmissionLimitMaxAttempts { get; init; } = 5;
    public IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields { get; init; } = [];
    public string PublicUrl { get; init; } = string.Empty;
    public string TrackableUrl { get; init; } = string.Empty;
}

public sealed class LandingPageCustomFieldDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool Required { get; init; }
}

public sealed class PublicLeadSubmissionDto
{
    public Guid DonorId { get; init; }
    public Guid? DonationId { get; init; }
    public bool Created { get; init; }
}

public sealed class LandingPageMetricsDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
    public int PromisesCount { get; init; }
    public int ConfirmedDonationsCount { get; init; }
    public decimal PromisedAmount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public decimal ConversionRate { get; init; }
    public IReadOnlyCollection<LandingPageSourceMetricDto> Sources { get; init; } = [];
    public IReadOnlyCollection<LandingPageDailyMetricDto> Daily { get; init; } = [];
}

public sealed class LandingPageSourceMetricDto
{
    public string Source { get; init; } = string.Empty;
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
}

public sealed class LandingPageDailyMetricDto
{
    public string Date { get; init; } = string.Empty;
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
}

public sealed class LandingPageTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal? GoalAmount { get; init; }
    public bool IsActive { get; init; }
    public int Version { get; init; }
    public IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields { get; init; } = [];
}

public sealed class LandingPageAuditEntryDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CreatedByUserId { get; init; }
    public string? CreatedByUserName { get; init; }
    public string? CreatedByUserEmail { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
}

public sealed class LandingPageTemplateDetailDto
{
    public LandingPageTemplateDto Template { get; init; } = new();
    public IReadOnlyCollection<LandingPageTemplateUsageDto> Uses { get; init; } = [];
    public IReadOnlyCollection<LandingPageTemplateVersionDto> Versions { get; init; } = [];
    public IReadOnlyCollection<LandingPageAuditEntryDto> AuditEntries { get; init; } = [];
}

public sealed class LandingPageTemplateVersionDto
{
    public Guid Id { get; init; }
    public int Version { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal? GoalAmount { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields { get; init; } = [];
    public string? CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed class LandingPageTemplateUsageDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAtUtc { get; init; }
}

public sealed class ApplyLandingPageTemplateResultDto
{
    public int UpdatedCount { get; init; }
}

public sealed class LandingPageAbuseReportDto
{
    public int AttemptsCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyCollection<LandingPageAbuseReportItemDto> Items { get; init; } = [];
}

public sealed class LandingPageAbuseReportItemDto
{
    public Guid Id { get; init; }
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string? FingerprintHash { get; init; }
    public Guid? ActiveBlockRuleId { get; init; }
    public bool Blocked { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset AttemptedAtUtc { get; init; }
}

public sealed class LandingPageLeadDto
{
    public Guid DonorId { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? UtmSource { get; init; }
    public Guid? DonationId { get; init; }
    public decimal? PromisedAmount { get; init; }
    public string? DonationStatus { get; init; }
}
