using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageMetrics;

public sealed record GetLandingPageMetricsQuery(string TargetType, Guid TargetId) : IRequest<LandingPageMetricsDto>;

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

        var leadEntries = await _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry =>
                entry.RelatedEntityType == targetType &&
                entry.RelatedEntityId == request.TargetId &&
                entry.Title == "Interesse pela landing page")
            .Select(entry => entry.Description ?? string.Empty)
            .ToListAsync(cancellationToken);

        var donations = await LandingDonations(targetType, request.TargetId).ToListAsync(cancellationToken);
        var confirmedDonations = donations.Where(donation => donation.Status == DonationStatus.Confirmed).ToArray();

        return new LandingPageMetricsDto
        {
            TargetType = targetType,
            TargetId = request.TargetId,
            LeadsCount = leadEntries.Count,
            PromisesCount = donations.Count(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue),
            ConfirmedDonationsCount = confirmedDonations.Length,
            PromisedAmount = donations
                .Where(donation => donation.Status is DonationStatus.Pending or DonationStatus.Overdue)
                .Sum(donation => donation.Amount),
            ConfirmedAmount = confirmedDonations.Sum(donation => donation.Amount),
            ConversionRate = leadEntries.Count == 0 ? 0 : Math.Round(confirmedDonations.Select(donation => donation.DonorId).Distinct().Count() / (decimal)leadEntries.Count * 100, 2),
            Sources = leadEntries
                .GroupBy(LandingPageContent.SourceFromTimeline)
                .Select(group => new LandingPageSourceMetricDto { Source = group.Key, LeadsCount = group.Count() })
                .OrderByDescending(metric => metric.LeadsCount)
                .ToArray(),
        };
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
}
