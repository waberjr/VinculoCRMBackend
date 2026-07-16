using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetPublicLandingPage;

public sealed record GetPublicLandingPageQuery(string Kind, Guid Id, bool IncludeDraft = false) : IRequest<PublicLandingPageDto?>;

public sealed class GetPublicLandingPageQueryHandler : IRequestHandler<GetPublicLandingPageQuery, PublicLandingPageDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetPublicLandingPageQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<PublicLandingPageDto?> Handle(GetPublicLandingPageQuery request, CancellationToken cancellationToken)
    {
        var page = request.Kind.Equals("campaign", StringComparison.OrdinalIgnoreCase)
            ? await CampaignLanding(request.Id, request.IncludeDraft, cancellationToken)
            : await ProjectLanding(request.Id, request.IncludeDraft, cancellationToken);

        return page is null ? null : await ResolveHeroImageUrl(page, cancellationToken);
    }

    private async Task<PublicLandingPageDto?> CampaignLanding(Guid id, bool includeDraft, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null || campaign.IsDeleted)
        {
            return null;
        }

        var landing = await LandingPage("campaign", id, includeDraft, cancellationToken);
        if (!includeDraft && landing is null)
        {
            return null;
        }

        var confirmedDonations = _context.Donations
            .AsNoTracking()
            .Where(donation =>
                donation.CampaignId == campaign.Id &&
                donation.Status == DonationStatus.Confirmed &&
                donation.PaidAtUtc != null);

        return new PublicLandingPageDto
        {
            Kind = "campaign",
            Id = campaign.Id,
            Title = landing?.Title ?? campaign.Name,
            Description = landing?.Subtitle ?? campaign.Description,
            HeroImageUrl = landing?.HeroImageUrl,
            GoalAmount = landing?.GoalAmount ?? campaign.GoalAmount ?? 0,
            CustomFields = LandingPageContent.ParseFields(landing?.CustomFieldsJson),
            ConfirmedAmount = await confirmedDonations.SumAsync(donation => (decimal?)donation.Amount, cancellationToken) ?? 0,
            DonorsCount = await confirmedDonations.Select(donation => donation.DonorId).Distinct().CountAsync(cancellationToken),
            StartDateUtc = campaign.StartDateUtc,
            EndDateUtc = campaign.EndDateUtc,
        };
    }

    private async Task<PublicLandingPageDto?> ProjectLanding(Guid id, bool includeDraft, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return null;
        }

        var landing = await LandingPage("project", id, includeDraft, cancellationToken);
        if (!includeDraft && landing is null)
        {
            return null;
        }

        var confirmedDonations = _context.DonationProjects
            .AsNoTracking()
            .Where(link =>
                link.ProjectId == project.Id &&
                link.Donation.Status == DonationStatus.Confirmed &&
                link.Donation.PaidAtUtc != null);

        return new PublicLandingPageDto
        {
            Kind = "project",
            Id = project.Id,
            Title = landing?.Title ?? project.Name,
            Description = landing?.Subtitle ?? project.Description,
            HeroImageUrl = landing?.HeroImageUrl,
            GoalAmount = landing?.GoalAmount ?? project.GoalAmount ?? 0,
            CustomFields = LandingPageContent.ParseFields(landing?.CustomFieldsJson),
            ConfirmedAmount = await confirmedDonations.SumAsync(link => (decimal?)link.Donation.Amount, cancellationToken) ?? 0,
            DonorsCount = await confirmedDonations.Select(link => link.Donation.DonorId).Distinct().CountAsync(cancellationToken),
            StartDateUtc = project.StartDateUtc,
            EndDateUtc = project.EndDateUtc,
        };
    }

    private async Task<LandingPage?> LandingPage(string targetType, Guid targetId, bool includeDraft, CancellationToken cancellationToken)
    {
        return await _context.LandingPages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(page =>
                !page.IsDeleted &&
                page.TargetType == targetType &&
                page.TargetId == targetId &&
                (includeDraft || (page.IsActive && page.IsPublished)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PublicLandingPageDto> ResolveHeroImageUrl(PublicLandingPageDto page, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(page.HeroImageUrl) ||
            !page.HeroImageUrl.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            return page;
        }

        var accessUrl = await _fileStorage.CreateTemporaryReadUrlAsync(page.HeroImageUrl, TimeSpan.FromHours(2), cancellationToken);
        return new PublicLandingPageDto
        {
            Kind = page.Kind,
            Id = page.Id,
            Title = page.Title,
            Description = page.Description,
            HeroImageUrl = accessUrl?.Url,
            GoalAmount = page.GoalAmount,
            ConfirmedAmount = page.ConfirmedAmount,
            DonorsCount = page.DonorsCount,
            StartDateUtc = page.StartDateUtc,
            EndDateUtc = page.EndDateUtc,
            CustomFields = page.CustomFields,
        };
    }
}
