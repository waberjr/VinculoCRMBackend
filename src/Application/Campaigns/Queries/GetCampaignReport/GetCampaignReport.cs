using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetCampaignReport;

public sealed record GetCampaignReportQuery(
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc,
    string? Status) : IRequest<CampaignReportDto>;

public sealed class GetCampaignReportQueryHandler : IRequestHandler<GetCampaignReportQuery, CampaignReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetCampaignReportQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<CampaignReportDto> Handle(GetCampaignReportQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var campaignsQuery = _context.Campaigns.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<CampaignStatus>(request.Status);
            campaignsQuery = campaignsQuery.Where(campaign => campaign.Status == status);
        }

        var campaigns = await campaignsQuery
            .Select(campaign => new
            {
                campaign.Id,
                campaign.Name,
                campaign.Status,
                GoalAmount = campaign.GoalAmount ?? 0,
            })
            .ToListAsync(cancellationToken);

        var campaignIds = campaigns.Select(campaign => campaign.Id).ToHashSet();
        var donationsQuery = _context.Donations
            .AsNoTracking()
            .Where(donation =>
                donation.CampaignId != null &&
                donation.Status == DonationStatus.Confirmed &&
                donation.PaidAtUtc != null);

        if (request.StartDateUtc is not null)
        {
            donationsQuery = donationsQuery.Where(donation => donation.PaidAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            donationsQuery = donationsQuery.Where(donation => donation.PaidAtUtc <= request.EndDateUtc);
        }

        var donations = (await donationsQuery
            .Select(donation => new
            {
                CampaignId = donation.CampaignId!.Value,
                donation.DonorId,
                donation.Amount,
                donation.PaidAtUtc,
            })
            .ToListAsync(cancellationToken))
            .Where(donation => campaignIds.Contains(donation.CampaignId))
            .ToList();

        var confirmedAmount = donations.Sum(donation => donation.Amount);
        var donationsCount = donations.Count;
        var donorsCount = donations.Select(donation => donation.DonorId).Distinct().Count();
        var campaignItems = campaigns
            .Select(campaign =>
            {
                var campaignDonations = donations.Where(donation => donation.CampaignId == campaign.Id).ToList();
                var campaignAmount = campaignDonations.Sum(donation => donation.Amount);
                var campaignDonationsCount = campaignDonations.Count;

                return new CampaignReportItemDto
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    Status = SystemOptionMapper.Code(campaign.Status),
                    GoalAmount = campaign.GoalAmount,
                    ConfirmedAmount = campaignAmount,
                    DonationsCount = campaignDonationsCount,
                    DonorsCount = campaignDonations.Select(donation => donation.DonorId).Distinct().Count(),
                    AverageDonationAmount = campaignDonationsCount == 0 ? 0 : campaignAmount / campaignDonationsCount,
                    GoalPercentage = campaign.GoalAmount <= 0 ? 0 : Math.Round(campaignAmount / campaign.GoalAmount * 100, 2),
                };
            })
            .OrderByDescending(campaign => campaign.ConfirmedAmount)
            .ThenBy(campaign => campaign.Name)
            .ToList();

        var periods = donations
            .Where(donation => donation.PaidAtUtc is not null)
            .GroupBy(donation => donation.PaidAtUtc!.Value.ToString("yyyy-MM"))
            .OrderBy(group => group.Key)
            .Select(group => new CampaignReportPeriodDto
            {
                Period = group.Key,
                ConfirmedAmount = group.Sum(donation => donation.Amount),
                DonationsCount = group.Count(),
                DonorsCount = group.Select(donation => donation.DonorId).Distinct().Count(),
            })
            .ToList();

        return new CampaignReportDto
        {
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            CampaignsCount = campaigns.Count,
            GoalAmount = campaigns.Sum(campaign => campaign.GoalAmount),
            ConfirmedAmount = confirmedAmount,
            DonationsCount = donationsCount,
            DonorsCount = donorsCount,
            AverageDonationAmount = donationsCount == 0 ? 0 : confirmedAmount / donationsCount,
            Campaigns = campaignItems,
            Periods = periods,
        };
    }
}
