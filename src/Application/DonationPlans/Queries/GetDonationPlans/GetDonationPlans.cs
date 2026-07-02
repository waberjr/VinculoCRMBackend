using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonationPlans.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.DonationPlans.Queries.GetDonationPlans;

public record GetDonationPlansQuery : IRequest<PaginatedResult<DonationPlanListItemDto>>
{
    public Guid? DonorId { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetDonationPlansQueryHandler : IRequestHandler<GetDonationPlansQuery, PaginatedResult<DonationPlanListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonationPlansQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<DonationPlanListItemDto>> Handle(GetDonationPlansQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.DonationPlans.AsNoTracking();

        if (request.DonorId is not null)
        {
            query = query.Where(plan => plan.DonorId == request.DonorId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<DonationPlanStatus>(request.Status);
            query = query.Where(plan => plan.Status == status);
        }

        var projected = query
            .OrderBy(plan => plan.BillingDay)
            .ThenBy(plan => plan.Donor.FullName)
            .Select(plan => new DonationPlanListItemDto
            {
                Id = plan.Id,
                DonorId = plan.DonorId,
                DonorName = plan.Donor.FullName,
                ExpectedAmount = plan.ExpectedAmount,
                BillingDay = plan.BillingDay,
                PreferredPaymentMethod = SystemOptionMapper.Name(plan.PreferredPaymentMethod),
                Status = SystemOptionMapper.Code(plan.Status),
                LastConfirmedAt = _context.Donations
                    .Where(donation => donation.DonationPlanId == plan.Id && donation.PaidAtUtc != null)
                    .Max(donation => (DateTimeOffset?)donation.PaidAtUtc),
                NextExpectedAt = NextExpectedAt(plan.BillingDay),
                CampaignName = plan.Campaign == null ? null : plan.Campaign.Name,
            });

        return await PaginatedResult<DonationPlanListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }

    private static DateTimeOffset NextExpectedAt(int billingDay)
    {
        var today = DateTimeOffset.UtcNow;
        var candidate = ExpectedAt(today.Year, today.Month, billingDay);
        return candidate.Date < today.Date ? ExpectedAt(today.AddMonths(1).Year, today.AddMonths(1).Month, billingDay) : candidate;
    }

    private static DateTimeOffset ExpectedAt(int year, int month, int billingDay)
    {
        var day = Math.Min(Math.Clamp(billingDay, 1, 31), DateTime.DaysInMonth(year, month));
        return new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero);
    }
}
