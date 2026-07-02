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

        var projected = query
            .OrderByDescending(campaign => campaign.StartDateUtc ?? campaign.Created)
            .ThenBy(campaign => campaign.Name)
            .Select(campaign => new CampaignListItemDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Type = SystemOptionMapper.Code(campaign.Type),
                Status = SystemOptionMapper.Code(campaign.Status),
                GoalAmount = campaign.GoalAmount ?? 0,
                ConfirmedAmount = _context.Donations
                    .Where(donation => donation.CampaignId == campaign.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0,
                DonorsCount = _context.Donations
                    .Where(donation => donation.CampaignId == campaign.Id)
                    .Select(donation => donation.DonorId)
                    .Distinct()
                    .Count(),
                DonationsCount = _context.Donations.Count(donation => donation.CampaignId == campaign.Id),
                StartDate = campaign.StartDateUtc,
                EndDate = campaign.EndDateUtc,
                AssignedUserName = string.IsNullOrWhiteSpace(campaign.AssignedUserId) ? "Sem responsavel" : campaign.AssignedUserId,
            });

        return await PaginatedResult<CampaignListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
