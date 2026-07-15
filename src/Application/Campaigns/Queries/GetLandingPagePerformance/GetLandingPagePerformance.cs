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

        var pages = await LandingPages(targetType).ToListAsync(cancellationToken);
        var targetKeys = pages.Select(page => $"{page.TargetType}:{page.TargetId:N}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var views = await LandingViews(request, targetKeys).ToListAsync(cancellationToken);
        var leads = (await LandingLeads(request, targetKeys).ToListAsync(cancellationToken))
            .Where(lead => string.IsNullOrWhiteSpace(request.Source) || LandingPageContent.SourceFromTimeline(lead.Description).Equals(request.Source.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var donations = await LandingDonations(pages.Select(page => page.TargetId).ToArray()).ToListAsync(cancellationToken);
        var confirmedDonations = donations.Where(donation => donation.Status == DonationStatus.Confirmed).ToArray();

        var items = pages
            .Select(page =>
            {
                var key = $"{page.TargetType}:{page.TargetId:N}";
                var pageViews = views.Where(view => view.TargetKey == key).ToArray();
                var pageLeads = leads.Where(lead => lead.TargetKey == key).ToArray();
                var pageDonations = donations.Where(donation => donation.TargetKey == key).ToArray();
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
                    ConversionRate = pageLeads.Length == 0 ? 0 : Math.Round(pageConfirmed.Select(donation => donation.DonorId).Distinct().Count() * 100m / pageLeads.Length, 2),
                    TopSource = TopSource(pageViews, pageLeads),
                };
            })
            .OrderByDescending(item => item.ViewsCount + item.LeadsCount)
            .ThenBy(item => item.TargetName)
            .ToArray();

        return new LandingPagePerformanceDto
        {
            ViewsCount = views.Count,
            LeadsCount = leads.Length,
            PromisesCount = donations.Count(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue),
            ConfirmedDonationsCount = confirmedDonations.Length,
            ConfirmedAmount = confirmedDonations.Sum(donation => donation.Amount),
            ConversionRate = leads.Length == 0 ? 0 : Math.Round(confirmedDonations.Select(donation => donation.DonorId).Distinct().Count() * 100m / leads.Length, 2),
            Items = items,
            Sources = SourceMetrics(views, leads),
            Utms = UtmMetrics(views, leads),
        };
    }

    private IQueryable<LandingPageProjection> LandingPages(string? targetType)
    {
        var campaigns = _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == "campaign" && (targetType == null || page.TargetType == targetType))
            .Join(_context.Campaigns.AsNoTracking(), page => page.TargetId, campaign => campaign.Id, (page, campaign) => new LandingPageProjection(page.TargetType, page.TargetId, campaign.Name, page.Title, page.IsActive, page.IsPublished));

        var projects = _context.LandingPages
            .AsNoTracking()
            .Where(page => page.TargetType == "project" && (targetType == null || page.TargetType == targetType))
            .Join(_context.Projects.AsNoTracking(), page => page.TargetId, project => project.Id, (page, project) => new LandingPageProjection(page.TargetType, page.TargetId, project.Name, page.Title, page.IsActive, page.IsPublished));

        return campaigns.Concat(projects);
    }

    private IQueryable<ViewProjection> LandingViews(GetLandingPagePerformanceQuery request, HashSet<string> targetKeys)
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
                view.TargetType + ":" + view.TargetId.ToString("N"),
                view.Source ?? view.UtmSource ?? "landing",
                view.UtmSource,
                view.UtmMedium,
                view.UtmCampaign))
            .Where(view => targetKeys.Contains(view.TargetKey));
    }

    private IQueryable<LeadProjection> LandingLeads(GetLandingPagePerformanceQuery request, HashSet<string> targetKeys)
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
                entry.RelatedEntityType + ":" + entry.RelatedEntityId!.Value.ToString("N"),
                entry.Description ?? string.Empty))
            .Where(lead => targetKeys.Contains(lead.TargetKey));
    }

    private IQueryable<DonationProjection> LandingDonations(Guid[] targetIds)
    {
        var campaignDonations = _context.Donations
            .AsNoTracking()
            .Where(donation => donation.CampaignId != null && targetIds.Contains(donation.CampaignId.Value))
            .Select(donation => new DonationProjection(
                "campaign:" + donation.CampaignId!.Value.ToString("N"),
                donation.DonorId,
                donation.Amount,
                donation.Status));

        var projectDonations = _context.DonationProjects
            .AsNoTracking()
            .Where(link => targetIds.Contains(link.ProjectId))
            .Select(link => new DonationProjection(
                "project:" + link.ProjectId.ToString("N"),
                link.Donation.DonorId,
                link.Donation.Amount,
                link.Donation.Status));

        return campaignDonations.Concat(projectDonations);
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

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? "nao informado" : value.Trim();

    private sealed record LandingPageProjection(string TargetType, Guid TargetId, string TargetName, string Title, bool IsActive, bool IsPublished);
    private sealed record ViewProjection(string TargetKey, string Source, string? UtmSource, string? UtmMedium, string? UtmCampaign);
    private sealed record LeadProjection(string TargetKey, string Description);
    private sealed record DonationProjection(string TargetKey, Guid DonorId, decimal Amount, DonationStatus Status);
    private sealed record UtmKey(string Source, string Medium, string Campaign);
}

public sealed class LandingPagePerformanceDto
{
    public int ViewsCount { get; init; }
    public int LeadsCount { get; init; }
    public int PromisesCount { get; init; }
    public int ConfirmedDonationsCount { get; init; }
    public decimal ConfirmedAmount { get; init; }
    public decimal ConversionRate { get; init; }
    public IReadOnlyCollection<LandingPagePerformanceItemDto> Items { get; init; } = [];
    public IReadOnlyCollection<LandingSourcePerformanceDto> Sources { get; init; } = [];
    public IReadOnlyCollection<LandingUtmPerformanceDto> Utms { get; init; } = [];
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
    public decimal ConversionRate { get; init; }
    public string TopSource { get; init; } = string.Empty;
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
