using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageMetrics;

public sealed record GetLandingPageMetricsQuery(
    string TargetType,
    Guid TargetId,
    string? Source = null,
    string? Status = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null) : IRequest<LandingPageMetricsDto>;

public sealed class GetLandingPageMetricsQueryHandler : IRequestHandler<GetLandingPageMetricsQuery, LandingPageMetricsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageMetricsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPageMetricsDto> Handle(GetLandingPageMetricsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var targetType = request.TargetType.Trim().ToLowerInvariant();

        var leadQuery = _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry =>
                entry.RelatedEntityType == targetType &&
                entry.RelatedEntityId == request.TargetId &&
                entry.Title == "Interesse pela landing page");

        if (request.StartDateUtc is not null)
        {
            leadQuery = leadQuery.Where(entry => entry.OccurredAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            leadQuery = leadQuery.Where(entry => entry.OccurredAtUtc <= request.EndDateUtc);
        }

        var leadEntries = (await leadQuery
            .Select(entry => new LeadMetricEntry(entry.OccurredAtUtc, entry.Description ?? string.Empty))
            .ToListAsync(cancellationToken))
            .Where(entry => string.IsNullOrWhiteSpace(request.Source) || LandingPageContent.SourceFromTimeline(entry.Description).Equals(request.Source.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var donations = (await LandingDonations(targetType, request.TargetId).ToListAsync(cancellationToken))
            .Where(donation => string.IsNullOrWhiteSpace(request.Status) || donation.Status.ToString().Equals(request.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var confirmedDonations = donations.Where(donation => donation.Status == DonationStatus.Confirmed).ToArray();
        var viewEntries = await LandingViews(targetType, request.TargetId, request.Source, request.StartDateUtc, request.EndDateUtc)
            .Select(view => new ViewMetricEntry(view.ViewedAtUtc, view.Source ?? view.UtmSource ?? "landing"))
            .ToListAsync(cancellationToken);

        return new LandingPageMetricsDto
        {
            TargetType = targetType,
            TargetId = request.TargetId,
            ViewsCount = viewEntries.Count,
            LeadsCount = leadEntries.Length,
            PromisesCount = donations.Count(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue),
            ConfirmedDonationsCount = confirmedDonations.Length,
            PromisedAmount = donations
                .Where(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue)
                .Sum(donation => donation.Amount),
            ConfirmedAmount = confirmedDonations.Sum(donation => donation.Amount),
            ConversionRate = leadEntries.Length == 0 ? 0 : Math.Round(confirmedDonations.Select(donation => donation.DonorId).Distinct().Count() / (decimal)leadEntries.Length * 100, 2),
            Sources = SourceMetrics(viewEntries, leadEntries),
            Daily = DailyMetrics(viewEntries, leadEntries),
        };
    }

    private static IReadOnlyCollection<LandingPageSourceMetricDto> SourceMetrics(
        IReadOnlyCollection<ViewMetricEntry> viewEntries,
        IReadOnlyCollection<LeadMetricEntry> leadEntries)
    {
        var viewGroups = viewEntries
            .GroupBy(entry => NormalizeSource(entry.Source))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var leadGroups = leadEntries
            .GroupBy(entry => LandingPageContent.SourceFromTimeline(entry.Description))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return viewGroups.Keys
            .Concat(leadGroups.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(source => new LandingPageSourceMetricDto
            {
                Source = source,
                ViewsCount = viewGroups.GetValueOrDefault(source),
                LeadsCount = leadGroups.GetValueOrDefault(source),
            })
            .OrderByDescending(metric => metric.ViewsCount + metric.LeadsCount)
            .ThenBy(metric => metric.Source)
            .ToArray();
    }

    private static IReadOnlyCollection<LandingPageDailyMetricDto> DailyMetrics(
        IReadOnlyCollection<ViewMetricEntry> viewEntries,
        IReadOnlyCollection<LeadMetricEntry> leadEntries)
    {
        var viewGroups = viewEntries
            .GroupBy(entry => entry.ViewedAtUtc.UtcDateTime.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var leadGroups = leadEntries
            .GroupBy(entry => entry.OccurredAtUtc.UtcDateTime.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return viewGroups.Keys
            .Concat(leadGroups.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(date => date)
            .Select(date => new LandingPageDailyMetricDto
            {
                Date = date,
                ViewsCount = viewGroups.GetValueOrDefault(date),
                LeadsCount = leadGroups.GetValueOrDefault(date),
            })
            .ToArray();
    }

    private static string NormalizeSource(string? source)
        => string.IsNullOrWhiteSpace(source) ? "landing" : source.Trim();

    private IQueryable<LandingPageView> LandingViews(string targetType, Guid targetId, string? source, DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
    {
        var query = _context.LandingPageViews
            .AsNoTracking()
            .Where(view => view.TargetType == targetType && view.TargetId == targetId);

        if (!string.IsNullOrWhiteSpace(source))
        {
            query = query.Where(view => view.Source == source || view.UtmSource == source);
        }

        if (startDateUtc is not null)
        {
            query = query.Where(view => view.ViewedAtUtc >= startDateUtc);
        }

        if (endDateUtc is not null)
        {
            query = query.Where(view => view.ViewedAtUtc <= endDateUtc);
        }

        return query;
    }

    private IQueryable<DonationProjection> LandingDonations(string targetType, Guid targetId)
    {
        if (targetType == "project")
        {
            return _context.DonationProjects
                .AsNoTracking()
                .Where(link => link.ProjectId == targetId)
                .Select(link => new DonationProjection(
                    link.Donation.Id,
                    link.Donation.DonorId,
                    link.Donation.Amount,
                    link.Donation.Status));
        }

        return _context.Donations
            .AsNoTracking()
            .Where(donation => donation.CampaignId == targetId)
            .Select(donation => new DonationProjection(
                donation.Id,
                donation.DonorId,
                donation.Amount,
                donation.Status));
    }

    private sealed record DonationProjection(Guid Id, Guid DonorId, decimal Amount, DonationStatus Status);
    private sealed record ViewMetricEntry(DateTimeOffset ViewedAtUtc, string Source);
    private sealed record LeadMetricEntry(DateTimeOffset OccurredAtUtc, string Description);
}
