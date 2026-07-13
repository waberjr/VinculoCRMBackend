using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetCampaigns;

public record GetCampaignsQuery : IRequest<PaginatedResult<CampaignListItemDto>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, PaginatedResult<CampaignListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetCampaignsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<CampaignListItemDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.Campaigns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(campaign =>
                campaign.Name.ToLower().Contains(search) ||
                (campaign.Description != null && campaign.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<CampaignStatus>(request.Status);
            query = query.Where(campaign => campaign.Status == status);
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(campaign => campaign.StartDateUtc ?? campaign.Created)
            .ThenBy(campaign => campaign.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(campaign => new
            {
                campaign.Id,
                campaign.Name,
                campaign.Type,
                campaign.Status,
                campaign.GoalAmount,
                campaign.StartDateUtc,
                campaign.EndDateUtc,
                campaign.AssignedUserId,
            })
            .ToListAsync(cancellationToken);

        var campaignIds = campaigns.Select(campaign => campaign.Id).ToArray();
        var campaignIdSet = campaignIds.ToHashSet();

        var campaignDonations = campaignIds.Length == 0
            ? []
            : (await _context.Donations
                .AsNoTracking()
                .Where(donation => donation.CampaignId.HasValue)
                .Select(donation => new
                {
                    CampaignId = donation.CampaignId!.Value,
                    donation.Status,
                    donation.PaidAtUtc,
                    donation.Amount,
                    donation.DonorId,
                })
                .ToListAsync(cancellationToken))
                .Where(donation => campaignIdSet.Contains(donation.CampaignId))
                .ToList();

        var donationMetrics = campaignDonations
            .GroupBy(donation => donation.CampaignId)
            .Select(group => new
            {
                CampaignId = group.Key,
                ConfirmedAmount = group
                    .Where(donation => donation.Status == DonationStatus.Confirmed && donation.PaidAtUtc != null)
                    .Sum(donation => donation.Amount),
                DonorsCount = group.Select(donation => donation.DonorId).Distinct().Count(),
                DonationsCount = group.Count(),
            })
            .ToList();

        var metricsByCampaignId = donationMetrics.ToDictionary(metric => metric.CampaignId);

        var items = campaigns
            .Select(campaign =>
            {
                metricsByCampaignId.TryGetValue(campaign.Id, out var metrics);

                return new CampaignListItemDto
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    Type = SystemOptionMapper.Code(campaign.Type),
                    Status = SystemOptionMapper.Code(campaign.Status),
                    GoalAmount = campaign.GoalAmount ?? 0,
                    ConfirmedAmount = metrics?.ConfirmedAmount ?? 0,
                    DonorsCount = metrics?.DonorsCount ?? 0,
                    DonationsCount = metrics?.DonationsCount ?? 0,
                    StartDate = campaign.StartDateUtc,
                    EndDate = campaign.EndDateUtc,
                    AssignedUserName = string.IsNullOrWhiteSpace(campaign.AssignedUserId) ? "Sem responsável" : campaign.AssignedUserId,
                };
            })
            .ToList();

        return new PaginatedResult<CampaignListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = totalCount,
        };
    }
}
