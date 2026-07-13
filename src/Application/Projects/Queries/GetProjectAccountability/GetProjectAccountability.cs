using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.ImpactProjects.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ImpactProjects.Queries.GetProjectAccountability;

public sealed record GetProjectAccountabilityQuery(
    Guid ProjectId,
    Guid? CampaignId,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc) : IRequest<ProjectAccountabilityDto?>;

public sealed class GetProjectAccountabilityQueryHandler : IRequestHandler<GetProjectAccountabilityQuery, ProjectAccountabilityDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProjectAccountabilityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectAccountabilityDto?> Handle(GetProjectAccountabilityQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Where(entity => entity.Id == request.ProjectId)
            .Select(entity => new
            {
                entity.Id,
                entity.Name,
                entity.Description,
                GoalAmount = entity.GoalAmount ?? 0,
                entity.StartDateUtc,
                entity.EndDateUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        var donationsQuery = _context.DonationProjects
            .AsNoTracking()
            .Where(link => link.ProjectId == request.ProjectId && link.Donation.Status == DonationStatus.Confirmed);

        if (request.CampaignId is not null)
        {
            donationsQuery = donationsQuery.Where(link => link.Donation.CampaignId == request.CampaignId);
        }

        if (request.StartDateUtc is not null)
        {
            donationsQuery = donationsQuery.Where(link => link.Donation.PaidAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            donationsQuery = donationsQuery.Where(link => link.Donation.PaidAtUtc <= request.EndDateUtc);
        }

        var donations = await donationsQuery
            .OrderByDescending(link => link.Donation.PaidAtUtc)
            .Select(link => new ProjectDonationAccountabilityDto
            {
                Id = link.DonationId,
                DonorId = link.Donation.DonorId,
                DonorName = link.Donation.Donor.FullName,
                CampaignId = link.Donation.CampaignId,
                CampaignName = link.Donation.Campaign == null ? null : link.Donation.Campaign.Name,
                ReceiptId = _context.Receipts
                    .Where(receipt => receipt.DonationId == link.DonationId && receipt.Status != ReceiptStatus.Cancelled)
                    .OrderByDescending(receipt => receipt.IssuedAtUtc)
                    .Select(receipt => (Guid?)receipt.Id)
                    .FirstOrDefault(),
                ReceiptNumber = _context.Receipts
                    .Where(receipt => receipt.DonationId == link.DonationId && receipt.Status != ReceiptStatus.Cancelled)
                    .OrderByDescending(receipt => receipt.IssuedAtUtc)
                    .Select(receipt => receipt.Number)
                    .FirstOrDefault(),
                Amount = link.Donation.Amount,
                PaidAtUtc = link.Donation.PaidAtUtc,
                Reference = link.Donation.Reference,
            })
            .ToListAsync(cancellationToken);

        var projectCampaigns = await _context.ProjectCampaigns
            .AsNoTracking()
            .Where(link => link.ProjectId == request.ProjectId)
            .OrderBy(link => link.Campaign.Name)
            .Select(link => new
            {
                link.CampaignId,
                CampaignName = link.Campaign.Name,
            })
            .ToListAsync(cancellationToken);

        var raisedAmount = donations.Sum(donation => donation.Amount);
        var campaigns = projectCampaigns
            .Where(campaign => request.CampaignId is null || campaign.CampaignId == request.CampaignId)
            .Select(campaign =>
            {
                var campaignDonations = donations
                    .Where(donation => donation.CampaignId == campaign.CampaignId)
                    .ToList();
                var campaignRaisedAmount = campaignDonations.Sum(donation => donation.Amount);

                return new ProjectCampaignAccountabilityDto
                {
                    Id = campaign.CampaignId,
                    Name = campaign.CampaignName,
                    RaisedAmount = campaignRaisedAmount,
                    DonationsCount = campaignDonations.Count,
                    DonorsCount = campaignDonations.Select(donation => donation.DonorId).Distinct().Count(),
                    AverageDonationAmount = campaignDonations.Count == 0 ? 0 : campaignRaisedAmount / campaignDonations.Count,
                    SharePercentage = raisedAmount == 0 ? 0 : Math.Round(campaignRaisedAmount / raisedAmount * 100, 2),
                };
            })
            .ToList();
        var filterCampaignName = request.CampaignId is null
            ? null
            : projectCampaigns
                .Where(campaign => campaign.CampaignId == request.CampaignId)
                .Select(campaign => campaign.CampaignName)
                .FirstOrDefault();
        var availableCampaigns = projectCampaigns
            .Select(campaign => new ProjectCampaignDto
            {
                Id = campaign.CampaignId,
                Name = campaign.CampaignName,
            })
            .ToList();

        var impactUpdates = await _context.ImpactUpdates
            .AsNoTracking()
            .Where(update => update.ProjectId == request.ProjectId)
            .OrderByDescending(update => update.PublishedAtUtc)
            .Select(update => new ProjectImpactUpdateAccountabilityDto
            {
                Id = update.Id,
                Title = update.Title,
                Content = update.Content,
                PublishedAtUtc = update.PublishedAtUtc,
            })
            .ToListAsync(cancellationToken);

        if (request.StartDateUtc is not null)
        {
            impactUpdates = impactUpdates
                .Where(update => update.PublishedAtUtc >= request.StartDateUtc)
                .ToList();
        }

        if (request.EndDateUtc is not null)
        {
            impactUpdates = impactUpdates
                .Where(update => update.PublishedAtUtc <= request.EndDateUtc)
                .ToList();
        }

        return new ProjectAccountabilityDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Description = project.Description,
            GoalAmount = project.GoalAmount,
            RaisedAmount = raisedAmount,
            DonationsCount = donations.Count,
            DonorsCount = donations.Select(donation => donation.DonorId).Distinct().Count(),
            FilterCampaignId = request.CampaignId,
            FilterCampaignName = filterCampaignName,
            FilterStartDateUtc = request.StartDateUtc,
            FilterEndDateUtc = request.EndDateUtc,
            StartDateUtc = project.StartDateUtc,
            EndDateUtc = project.EndDateUtc,
            AvailableCampaigns = availableCampaigns,
            Campaigns = campaigns,
            Donations = donations,
            ImpactUpdates = impactUpdates,
        };
    }
}
