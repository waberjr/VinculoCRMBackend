using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageLeads;

public sealed record GetLandingPageLeadsQuery(
    string TargetType,
    Guid TargetId,
    string? Source = null,
    string? Status = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PaginatedResult<LandingPageLeadDto>>;

public sealed class GetLandingPageLeadsQueryHandler : IRequestHandler<GetLandingPageLeadsQuery, PaginatedResult<LandingPageLeadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetLandingPageLeadsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<LandingPageLeadDto>> Handle(GetLandingPageLeadsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 20 : request.PageSize, 1, 100);

        var query = _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry =>
                entry.RelatedEntityType == targetType &&
                entry.RelatedEntityId == request.TargetId &&
                entry.Title == "Interesse pela landing page")
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Select(entry => new
            {
                entry.DonorId,
                entry.OccurredAtUtc,
                Description = entry.Description ?? string.Empty,
                entry.Donor.FullName,
                entry.Donor.Email,
                entry.Donor.Phone,
            });

        if (request.StartDateUtc is not null)
        {
            query = query.Where(entry => entry.OccurredAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(entry => entry.OccurredAtUtc <= request.EndDateUtc);
        }

        var rows = await query.ToListAsync(cancellationToken);

        var donorIds = rows.Select(row => row.DonorId).Distinct().ToArray();
        var donationLookup = await LandingDonations(targetType, request.TargetId)
            .Where(donation => donorIds.Contains(donation.DonorId))
            .GroupBy(donation => donation.DonorId)
            .Select(group => group.OrderByDescending(donation => donation.Created).First())
            .ToDictionaryAsync(donation => donation.DonorId, cancellationToken);

        var filteredItems = rows.Select(row =>
        {
            donationLookup.TryGetValue(row.DonorId, out var donation);
            return new LandingPageLeadDto
            {
                DonorId = row.DonorId,
                DonorName = row.FullName,
                Email = row.Email,
                Phone = row.Phone,
                CreatedAtUtc = row.OccurredAtUtc,
                Source = LandingPageContent.SourceFromTimeline(row.Description),
                UtmSource = LandingPageContent.UtmSourceFromTimeline(row.Description),
                DonationId = donation?.Id,
                PromisedAmount = donation?.Amount,
                DonationStatus = donation?.Status.ToString(),
            };
        })
        .Where(lead => string.IsNullOrWhiteSpace(request.Source) || lead.Source.Equals(request.Source.Trim(), StringComparison.OrdinalIgnoreCase))
        .Where(lead => string.IsNullOrWhiteSpace(request.Status) || (lead.DonationStatus ?? "Lead").Equals(request.Status.Trim(), StringComparison.OrdinalIgnoreCase))
        .ToArray();
        var items = filteredItems
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return new PaginatedResult<LandingPageLeadDto>
        {
            Items = items,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(filteredItems.Length / (double)pageSize),
            TotalCount = filteredItems.Length,
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
                    link.Donation.Status,
                    link.Donation.Created));
        }

        return _context.Donations
            .AsNoTracking()
            .Where(donation => donation.CampaignId == targetId)
            .Select(donation => new DonationProjection(
                donation.Id,
                donation.DonorId,
                donation.Amount,
                donation.Status,
                donation.Created));
    }

    private sealed record DonationProjection(Guid Id, Guid DonorId, decimal Amount, DonationStatus Status, DateTimeOffset Created);
}
