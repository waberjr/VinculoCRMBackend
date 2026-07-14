using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donations.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donations.Queries.GetDonations;

public record GetDonationsQuery : IRequest<PaginatedResult<DonationListItemDto>>
{
    public string? Search { get; init; }
    public Guid? DonorId { get; init; }
    public string? DonorName { get; init; }
    public Guid? CampaignId { get; init; }
    public string? CampaignName { get; init; }
    public Guid? ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public string? Status { get; init; }
    public string? Type { get; init; }
    public string? PaymentMethod { get; init; }
    public string? AmountRange { get; init; }
    public string? DateReference { get; init; }
    public DateTimeOffset? FromUtc { get; init; }
    public DateTimeOffset? ToUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetDonationsQueryHandler : IRequestHandler<GetDonationsQuery, PaginatedResult<DonationListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonationsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<DonationListItemDto>> Handle(GetDonationsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.Donations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(donation =>
                donation.Donor.FullName.ToLower().Contains(search) ||
                (donation.Reference != null && donation.Reference.ToLower().Contains(search)) ||
                _context.DonationProjects.Any(projectLink =>
                    projectLink.DonationId == donation.Id &&
                    projectLink.Project.Name.ToLower().Contains(search)));
        }

        if (request.DonorId is not null) query = query.Where(donation => donation.DonorId == request.DonorId);
        if (!string.IsNullOrWhiteSpace(request.DonorName))
        {
            var donorName = request.DonorName.Trim().ToLower();
            query = query.Where(donation => donation.Donor.FullName.ToLower() == donorName);
        }

        if (request.CampaignId is not null) query = query.Where(donation => donation.CampaignId == request.CampaignId);
        if (!string.IsNullOrWhiteSpace(request.CampaignName))
        {
            var campaignName = request.CampaignName.Trim().ToLower();
            query = query.Where(donation => donation.Campaign != null && donation.Campaign.Name.ToLower() == campaignName);
        }

        if (request.ProjectId is not null)
        {
            query = query.Where(donation =>
                _context.DonationProjects.Any(projectLink =>
                    projectLink.DonationId == donation.Id &&
                    projectLink.ProjectId == request.ProjectId));
        }
        if (!string.IsNullOrWhiteSpace(request.ProjectName))
        {
            var projectName = request.ProjectName.Trim().ToLower();
            query = query.Where(donation =>
                _context.DonationProjects.Any(projectLink =>
                    projectLink.DonationId == donation.Id &&
                    projectLink.Project.Name.ToLower() == projectName));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<DonationStatus>(request.Status);
            query = query.Where(donation => donation.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var type = SystemOptionMapper.Parse<DonationType>(request.Type);
            query = query.Where(donation => donation.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            var paymentMethod = SystemOptionMapper.Parse<PaymentMethod>(request.PaymentMethod);
            query = query.Where(donation => donation.PaymentMethod == paymentMethod);
        }

        query = ApplyAmountRangeFilter(query, request.AmountRange);
        query = ApplyDateFilters(query, request.DateReference, request.FromUtc, request.ToUtc);

        var projected = query
            .OrderByDescending(donation => donation.PaidAtUtc ?? donation.ExpectedAtUtc ?? donation.Created)
            .Select(donation => new DonationListItemDto
            {
                Id = donation.Id,
                DonorId = donation.DonorId,
                DonorName = donation.Donor.FullName,
                CampaignId = donation.CampaignId,
                CampaignName = donation.Campaign == null ? null : donation.Campaign.Name,
                ProjectId = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == donation.Id)
                    .Select(projectLink => (Guid?)projectLink.ProjectId)
                    .FirstOrDefault(),
                ProjectName = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == donation.Id)
                    .Select(projectLink => projectLink.Project.Name)
                    .FirstOrDefault(),
                ReceiptId = _context.Receipts
                    .Where(receipt => receipt.DonationId == donation.Id && receipt.Status != ReceiptStatus.Cancelled)
                    .Select(receipt => (Guid?)receipt.Id)
                    .FirstOrDefault(),
                ReceiptNumber = _context.Receipts
                    .Where(receipt => receipt.DonationId == donation.Id && receipt.Status != ReceiptStatus.Cancelled)
                    .Select(receipt => receipt.Number)
                    .FirstOrDefault(),
                Amount = donation.Amount,
                ExpectedAtUtc = donation.ExpectedAtUtc,
                PaidAtUtc = donation.PaidAtUtc,
                Reference = donation.Reference,
                Type = SystemOptionMapper.ToOptionDto(donation.Type),
                Status = SystemOptionMapper.ToOptionDto(donation.Status),
                PaymentMethod = SystemOptionMapper.ToOptionDto(donation.PaymentMethod),
            });

        return await PaginatedResult<DonationListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }

    private static IQueryable<Domain.Entities.Donation> ApplyAmountRangeFilter(IQueryable<Domain.Entities.Donation> query, string? amountRange)
    {
        return amountRange switch
        {
            "UpTo100" => query.Where(donation => donation.Amount <= 100),
            "From100To1000" => query.Where(donation => donation.Amount > 100 && donation.Amount <= 1000),
            "Above1000" => query.Where(donation => donation.Amount > 1000),
            _ => query,
        };
    }

    private static IQueryable<Domain.Entities.Donation> ApplyDateFilters(
        IQueryable<Domain.Entities.Donation> query,
        string? dateReference,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        return dateReference switch
        {
            "Expected" => ApplyExpectedDateRange(query, fromUtc, toUtc),
            "Paid" => ApplyPaidDateRange(query, fromUtc, toUtc),
            _ => ApplyAnyDateRange(query, fromUtc, toUtc),
        };
    }

    private static IQueryable<Domain.Entities.Donation> ApplyAnyDateRange(
        IQueryable<Domain.Entities.Donation> query,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        if (fromUtc is not null)
        {
            query = query.Where(donation => donation.ExpectedAtUtc >= fromUtc || donation.PaidAtUtc >= fromUtc);
        }

        if (toUtc is not null)
        {
            query = query.Where(donation => donation.ExpectedAtUtc <= toUtc || donation.PaidAtUtc <= toUtc);
        }

        return query;
    }

    private static IQueryable<Domain.Entities.Donation> ApplyExpectedDateRange(
        IQueryable<Domain.Entities.Donation> query,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        if (fromUtc is not null)
        {
            query = query.Where(donation => donation.ExpectedAtUtc >= fromUtc);
        }

        if (toUtc is not null)
        {
            query = query.Where(donation => donation.ExpectedAtUtc <= toUtc);
        }

        return query;
    }

    private static IQueryable<Domain.Entities.Donation> ApplyPaidDateRange(
        IQueryable<Domain.Entities.Donation> query,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        if (fromUtc is not null)
        {
            query = query.Where(donation => donation.PaidAtUtc >= fromUtc);
        }

        if (toUtc is not null)
        {
            query = query.Where(donation => donation.PaidAtUtc <= toUtc);
        }

        return query;
    }
}
