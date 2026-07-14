using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetPublicLandingPage;

public sealed record GetPublicLandingPageQuery(string Kind, Guid Id) : IRequest<PublicLandingPageDto?>;

public sealed class GetPublicLandingPageQueryHandler : IRequestHandler<GetPublicLandingPageQuery, PublicLandingPageDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPublicLandingPageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicLandingPageDto?> Handle(GetPublicLandingPageQuery request, CancellationToken cancellationToken)
    {
        return request.Kind.Equals("campaign", StringComparison.OrdinalIgnoreCase)
            ? await CampaignLanding(request.Id, cancellationToken)
            : await ProjectLanding(request.Id, cancellationToken);
    }

    private async Task<PublicLandingPageDto?> CampaignLanding(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Campaigns
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(campaign => !campaign.IsDeleted && campaign.Id == id)
            .Select(campaign => new PublicLandingPageDto
            {
                Kind = "campaign",
                Id = campaign.Id,
                Title = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "campaign" && page.TargetId == campaign.Id)
                    .Select(page => page.Title)
                    .FirstOrDefault() ?? campaign.Name,
                Description = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "campaign" && page.TargetId == campaign.Id)
                    .Select(page => page.Subtitle)
                    .FirstOrDefault() ?? campaign.Description,
                HeroImageUrl = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "campaign" && page.TargetId == campaign.Id)
                    .Select(page => page.HeroImageUrl)
                    .FirstOrDefault(),
                GoalAmount = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "campaign" && page.TargetId == campaign.Id)
                    .Select(page => page.GoalAmount)
                    .FirstOrDefault() ?? campaign.GoalAmount ?? 0,
                ConfirmedAmount = _context.Donations
                    .Where(donation =>
                        donation.CampaignId == campaign.Id &&
                        donation.Status == DonationStatus.Confirmed &&
                        donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0,
                DonorsCount = _context.Donations
                    .Where(donation =>
                        donation.CampaignId == campaign.Id &&
                        donation.Status == DonationStatus.Confirmed &&
                        donation.PaidAtUtc != null)
                    .Select(donation => donation.DonorId)
                    .Distinct()
                    .Count(),
                StartDateUtc = campaign.StartDateUtc,
                EndDateUtc = campaign.EndDateUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PublicLandingPageDto?> ProjectLanding(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(project => !project.IsDeleted && project.Id == id)
            .Select(project => new PublicLandingPageDto
            {
                Kind = "project",
                Id = project.Id,
                Title = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "project" && page.TargetId == project.Id)
                    .Select(page => page.Title)
                    .FirstOrDefault() ?? project.Name,
                Description = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "project" && page.TargetId == project.Id)
                    .Select(page => page.Subtitle)
                    .FirstOrDefault() ?? project.Description,
                HeroImageUrl = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "project" && page.TargetId == project.Id)
                    .Select(page => page.HeroImageUrl)
                    .FirstOrDefault(),
                GoalAmount = _context.LandingPages
                    .IgnoreQueryFilters()
                    .Where(page => !page.IsDeleted && page.IsActive && page.TargetType == "project" && page.TargetId == project.Id)
                    .Select(page => page.GoalAmount)
                    .FirstOrDefault() ?? project.GoalAmount ?? 0,
                ConfirmedAmount = _context.DonationProjects
                    .Where(link =>
                        link.ProjectId == project.Id &&
                        link.Donation.Status == DonationStatus.Confirmed &&
                        link.Donation.PaidAtUtc != null)
                    .Sum(link => (decimal?)link.Donation.Amount) ?? 0,
                DonorsCount = _context.DonationProjects
                    .Where(link =>
                        link.ProjectId == project.Id &&
                        link.Donation.Status == DonationStatus.Confirmed &&
                        link.Donation.PaidAtUtc != null)
                    .Select(link => link.Donation.DonorId)
                    .Distinct()
                    .Count(),
                StartDateUtc = project.StartDateUtc,
                EndDateUtc = project.EndDateUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
