using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;

public sealed record GetLandingPagePerformanceQuery(
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null,
    string? TargetType = null,
    string? Source = null) : IRequest<LandingPagePerformanceDto>;

public sealed class GetLandingPagePerformanceQueryHandler : IRequestHandler<GetLandingPagePerformanceQuery, LandingPagePerformanceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPagePerformanceQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPagePerformanceDto> Handle(GetLandingPagePerformanceQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var targetType = string.IsNullOrWhiteSpace(request.TargetType) ? null : request.TargetType.Trim().ToLowerInvariant();

        var pages = await LandingPages(targetType, cancellationToken);
        var targetKeys = pages.Select(page => $"{page.TargetType}:{page.TargetId:N}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var views = (await LandingViews(request).ToListAsync(cancellationToken))
            .Where(view => targetKeys.Contains(view.TargetKey))
            .ToArray();
        var leads = (await LandingLeads(request).ToListAsync(cancellationToken))
            .Where(lead => targetKeys.Contains(lead.TargetKey))
            .Where(lead => string.IsNullOrWhiteSpace(request.Source) || LandingPageContent.SourceFromTimeline(lead.Description).Equals(request.Source.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var donations = (await LandingDonations(pages.Select(page => page.TargetId).ToArray(), cancellationToken))
            .Where(donation => targetKeys.Contains(donation.TargetKey))
            .ToArray();
        var attempts = (await LandingAttempts(request).ToListAsync(cancellationToken))
            .Where(attempt => targetKeys.Contains(attempt.TargetKey))
            .ToArray();
        var confirmedDonations = donations.Where(donation => donation.Status == DonationStatus.Confirmed).ToArray();

        var items = pages
            .Select(page =>
            {
                var key = $"{page.TargetType}:{page.TargetId:N}";
                var pageViews = views.Where(view => view.TargetKey == key).ToArray();
                var pageLeads = leads.Where(lead => lead.TargetKey == key).ToArray();
                var pageDonations = donations.Where(donation => donation.TargetKey == key).ToArray();
                var pageAttempts = attempts.Where(attempt => attempt.TargetKey == key).ToArray();
                var pageConfirmed = pageDonations.Where(donation => donation.Status == DonationStatus.Confirmed).ToArray();

                return new LandingPagePerformanceItemDto
                {
                    TargetType = page.TargetType,
                    TargetId = page.TargetId,
                    TargetName = page.TargetName,
                    Title = page.Title,
                    IsActive = page.IsActive,
                    IsPublished = page.IsPublished,
                    ViewsCount = pageViews.Length,
                    LeadsCount = pageLeads.Length,
                    PromisesCount = pageDonations.Count(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue),
                    ConfirmedDonationsCount = pageConfirmed.Length,
                    ConfirmedAmount = pageConfirmed.Sum(donation => donation.Amount),
                    BlockedSubmissionsCount = pageAttempts.Count(attempt => attempt.Blocked),
                    ConversionRate = pageLeads.Length == 0 ? 0 : Math.Round(pageConfirmed.Select(donation => donation.DonorId).Distinct().Count() * 100m / pageLeads.Length, 2),
                    TopSource = TopSource(pageViews, pageLeads),
                };
            })
            .OrderByDescending(item => item.ViewsCount + item.LeadsCount)
            .ThenBy(item => item.TargetName)
            .ToArray();

        return new LandingPagePerformanceDto
        {
            ViewsCount = views.Length,
            LeadsCount = leads.Length,
            PromisesCount = donations.Count(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue),
            ConfirmedDonationsCount = confirmedDonations.Length,
            ConfirmedAmount = confirmedDonations.Sum(donation => donation.Amount),
            SubmissionAttemptsCount = attempts.Length,
            BlockedSubmissionsCount = attempts.Count(attempt => attempt.Blocked),
            BlockRate = attempts.Length == 0 ? 0 : Math.Round(attempts.Count(attempt => attempt.Blocked) * 100m / attempts.Length, 2),
            ProtectionAlerts = ProtectionAlerts(items),
            ConversionRate = leads.Length == 0 ? 0 : Math.Round(confirmedDonations.Select(donation => donation.DonorId).Distinct().Count() * 100m / leads.Length, 2),
            Items = items,
            Sources = SourceMetrics(views, leads),
            Utms = UtmMetrics(views, leads),
            Daily = DailyMetrics(views, leads),
        };
    }

    private async Task<IReadOnlyCollection<LandingPageProjection>> LandingPages(string? targetType, CancellationToken cancellationToken)
    {
        var campaigns = await _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == "campaign" && (targetType == null || page.TargetType == targetType))
            .Join(_context.Campaigns.AsNoTracking(), page => page.TargetId, campaign => campaign.Id, (page, campaign) => new
            {
                page.TargetType,
                page.TargetId,
                TargetName = campaign.Name,
                page.Title,
                page.IsActive,
                page.IsPublished,
            })
            .ToListAsync(cancellationToken);

        var projects = await _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == "project" && (targetType == null || page.TargetType == targetType))
            .Join(_context.Projects.AsNoTracking(), page => page.TargetId, project => project.Id, (page, project) => new
            {
                page.TargetType,
                page.TargetId,
                TargetName = project.Name,
                page.Title,
                page.IsActive,
                page.IsPublished,
            })
            .ToListAsync(cancellationToken);

        return campaigns
            .Concat(projects)
            .Select(page => new LandingPageProjection(page.TargetType, page.TargetId, page.TargetName, page.Title, page.IsActive, page.IsPublished))
            .ToArray();
    }

    private IQueryable<ViewProjection> LandingViews(GetLandingPagePerformanceQuery request)
    {
        var query = _context.LandingPageViews.AsNoTracking();

        if (request.StartDateUtc is not null)
        {
            query = query.Where(view => view.ViewedAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(view => view.ViewedAtUtc <= request.EndDateUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            query = query.Where(view => view.Source == request.Source || view.UtmSource == request.Source);
        }

        return query
            .Select(view => new ViewProjection(
                view.TargetType,
                view.TargetId,
                view.ViewedAtUtc,
                view.Source ?? view.UtmSource ?? "landing",
                view.UtmSource,
                view.UtmMedium,
                view.UtmCampaign));
    }

    private IQueryable<LeadProjection> LandingLeads(GetLandingPagePerformanceQuery request)
    {
        var query = _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry => entry.Title == "Interesse pela landing page" && entry.RelatedEntityId != null);

        if (request.StartDateUtc is not null)
        {
            query = query.Where(entry => entry.OccurredAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(entry => entry.OccurredAtUtc <= request.EndDateUtc);
        }

        return query
            .Select(entry => new LeadProjection(
                entry.RelatedEntityType ?? string.Empty,
                entry.RelatedEntityId!.Value,
                entry.OccurredAtUtc,
                entry.Description ?? string.Empty));
    }

    private IQueryable<AttemptProjection> LandingAttempts(GetLandingPagePerformanceQuery request)
    {
        var query = _context.LandingPageSubmissionAttempts.AsNoTracking();
        if (request.StartDateUtc is not null)
        {
            query = query.Where(attempt => attempt.AttemptedAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(attempt => attempt.AttemptedAtUtc <= request.EndDateUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            query = query.Where(attempt => attempt.Source == request.Source);
        }

        return query.Select(attempt => new AttemptProjection(attempt.TargetType, attempt.TargetId, attempt.Blocked));
    }

    private async Task<IReadOnlyCollection<DonationProjection>> LandingDonations(Guid[] targetIds, CancellationToken cancellationToken)
    {
        var campaignDonations = await _context.Donations
            .AsNoTracking()
            .Where(donation => donation.CampaignId != null && targetIds.Contains(donation.CampaignId.Value))
            .Select(donation => new
            {
                TargetType = "campaign",
                TargetId = donation.CampaignId!.Value,
                donation.DonorId,
                donation.Amount,
                donation.Status,
            })
            .ToListAsync(cancellationToken);

        var projectDonations = await _context.DonationProjects
            .AsNoTracking()
            .Where(link => targetIds.Contains(link.ProjectId))
            .Select(link => new
            {
                TargetType = "project",
                TargetId = link.ProjectId,
                link.Donation.DonorId,
                link.Donation.Amount,
                link.Donation.Status,
            })
            .ToListAsync(cancellationToken);

        return campaignDonations
            .Concat(projectDonations)
            .Select(donation => new DonationProjection(donation.TargetType, donation.TargetId, donation.DonorId, donation.Amount, donation.Status))
            .ToArray();
    }

    private static string TopSource(IReadOnlyCollection<ViewProjection> views, IReadOnlyCollection<LeadProjection> leads)
        => SourceMetrics(views, leads).FirstOrDefault()?.Source ?? "landing";

    private static IReadOnlyCollection<LandingSourcePerformanceDto> SourceMetrics(
        IReadOnlyCollection<ViewProjection> views,
        IReadOnlyCollection<LeadProjection> leads)
    {
        var viewGroups = views.GroupBy(view => Normalize(view.Source)).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var leadGroups = leads.GroupBy(lead => LandingPageContent.SourceFromTimeline(lead.Description)).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return viewGroups.Keys
            .Concat(leadGroups.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(source => new LandingSourcePerformanceDto
            {
                Source = source,
                ViewsCount = viewGroups.GetValueOrDefault(source),
                LeadsCount = leadGroups.GetValueOrDefault(source),
            })
            .OrderByDescending(metric => metric.ViewsCount + metric.LeadsCount)
            .ThenBy(metric => metric.Source)
            .ToArray();
    }

    private static IReadOnlyCollection<LandingUtmPerformanceDto> UtmMetrics(
        IReadOnlyCollection<ViewProjection> views,
        IReadOnlyCollection<LeadProjection> leads)
    {
        var viewGroups = views
            .GroupBy(view => new UtmKey(Normalize(view.UtmSource), Normalize(view.UtmMedium), Normalize(view.UtmCampaign)))
            .ToDictionary(group => group.Key, group => group.Count());
        var leadGroups = leads
            .GroupBy(lead => new UtmKey(
                Normalize(LandingPageContent.UtmSourceFromTimeline(lead.Description)),
                "nao informado",
                "nao informado"))
            .ToDictionary(group => group.Key, group => group.Count());

        return viewGroups.Keys
            .Concat(leadGroups.Keys)
            .Distinct()
            .Select(key => new LandingUtmPerformanceDto
            {
                UtmSource = key.Source,
                UtmMedium = key.Medium,
                UtmCampaign = key.Campaign,
                ViewsCount = viewGroups.GetValueOrDefault(key),
                LeadsCount = leadGroups.GetValueOrDefault(key),
            })
            .OrderByDescending(metric => metric.ViewsCount + metric.LeadsCount)
            .ThenBy(metric => metric.UtmSource)
            .Take(20)
            .ToArray();
    }

    private static IReadOnlyCollection<LandingDailyPerformanceDto> DailyMetrics(
        IReadOnlyCollection<ViewProjection> views,
        IReadOnlyCollection<LeadProjection> leads)
    {
        var viewGroups = views
            .GroupBy(view => view.OccurredAtUtc.UtcDateTime.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var leadGroups = leads
            .GroupBy(lead => lead.OccurredAtUtc.UtcDateTime.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return viewGroups.Keys
            .Concat(leadGroups.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(date => date)
            .Select(date => new LandingDailyPerformanceDto
            {
                Date = date,
                ViewsCount = viewGroups.GetValueOrDefault(date),
                LeadsCount = leadGroups.GetValueOrDefault(date),
            })
            .ToArray();
    }

    private static IReadOnlyCollection<LandingProtectionAlertDto> ProtectionAlerts(IReadOnlyCollection<LandingPagePerformanceItemDto> items)
    {
        return items
            .Where(item => item.BlockedSubmissionsCount >= 5)
            .OrderByDescending(item => item.BlockedSubmissionsCount)
            .Take(10)
            .Select(item => new LandingProtectionAlertDto
            {
                TargetType = item.TargetType,
                TargetId = item.TargetId,
                TargetName = item.TargetName,
                BlockedSubmissionsCount = item.BlockedSubmissionsCount,
                Severity = item.BlockedSubmissionsCount >= 20 ? "high" : "medium",
            })
            .ToArray();
    }

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? "nao informado" : value.Trim();

    private sealed record LandingPageProjection(string TargetType, Guid TargetId, string TargetName, string Title, bool IsActive, bool IsPublished);
    private sealed record ViewProjection(string TargetType, Guid TargetId, DateTimeOffset OccurredAtUtc, string Source, string? UtmSource, string? UtmMedium, string? UtmCampaign)
    {
        public string TargetKey => $"{TargetType}:{TargetId:N}";
    }

    private sealed record LeadProjection(string TargetType, Guid TargetId, DateTimeOffset OccurredAtUtc, string Description)
    {
        public string TargetKey => $"{TargetType}:{TargetId:N}";
    }

    private sealed record DonationProjection(string TargetType, Guid TargetId, Guid DonorId, decimal Amount, DonationStatus Status)
    {
        public string TargetKey => $"{TargetType}:{TargetId:N}";
    }
    private sealed record AttemptProjection(string TargetType, Guid TargetId, bool Blocked)
    {
        public string TargetKey => $"{TargetType}:{TargetId:N}";
    }
    private sealed record UtmKey(string Source, string Medium, string Campaign);
}

public sealed class LandingPagePerformanceDto
{
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
    public int PromisesCount { get; init; }
    public int ConfirmedDonationsCount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int SubmissionAttemptsCount { get; init; }
    public int BlockedSubmissionsCount { get; init; }
    public decimal BlockRate { get; init; }
    public decimal ConversionRate { get; init; }
    public IReadOnlyCollection<LandingProtectionAlertDto> ProtectionAlerts { get; init; } = [];
    public IReadOnlyCollection<LandingPagePerformanceItemDto> Items { get; init; } = [];
    public IReadOnlyCollection<LandingSourcePerformanceDto> Sources { get; init; } = [];
    public IReadOnlyCollection<LandingUtmPerformanceDto> Utms { get; init; } = [];
    public IReadOnlyCollection<LandingDailyPerformanceDto> Daily { get; init; } = [];
}

public sealed class LandingPagePerformanceItemDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsPublished { get; init; }
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
    public int PromisesCount { get; init; }
    public int ConfirmedDonationsCount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public int BlockedSubmissionsCount { get; init; }
    public decimal ConversionRate { get; init; }
    public string TopSource { get; init; } = string.Empty;
}

public sealed class LandingProtectionAlertDto
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int BlockedSubmissionsCount { get; init; }
    public string Severity { get; init; } = "medium";
}

public sealed class LandingSourcePerformanceDto
{
    public string Source { get; init; } = string.Empty;
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
}

public sealed class LandingUtmPerformanceDto
{
    public string UtmSource { get; init; } = string.Empty;
    public string UtmMedium { get; init; } = string.Empty;
    public string UtmCampaign { get; init; } = string.Empty;
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
}

public sealed class LandingDailyPerformanceDto
{
    public string Date { get; init; } = string.Empty;
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
}
