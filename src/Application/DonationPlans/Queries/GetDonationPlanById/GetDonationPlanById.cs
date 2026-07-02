using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonationPlans.Models;

namespace VinculoBackend.Application.DonationPlans.Queries.GetDonationPlanById;

public record GetDonationPlanByIdQuery(Guid Id) : IRequest<DonationPlanListItemDto?>;

public sealed class GetDonationPlanByIdQueryHandler : IRequestHandler<GetDonationPlanByIdQuery, DonationPlanListItemDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonationPlanByIdQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<DonationPlanListItemDto?> Handle(GetDonationPlanByIdQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        return await _context.DonationPlans
            .AsNoTracking()
            .Where(plan => plan.Id == request.Id)
            .Select(plan => new DonationPlanListItemDto
            {
                Id = plan.Id,
                DonorId = plan.DonorId,
                DonorName = plan.Donor.FullName,
                CampaignId = plan.CampaignId,
                ExpectedAmount = plan.ExpectedAmount,
                BillingDay = plan.BillingDay,
                PreferredPaymentMethod = SystemOptionMapper.Code(plan.PreferredPaymentMethod),
                Status = SystemOptionMapper.Code(plan.Status),
                StartDateUtc = plan.StartDateUtc,
                LastConfirmedAt = _context.Donations
                    .Where(donation => donation.DonationPlanId == plan.Id && donation.PaidAtUtc != null)
                    .Max(donation => (DateTimeOffset?)donation.PaidAtUtc),
                NextExpectedAt = NextExpectedAt(plan.BillingDay),
                CampaignName = plan.Campaign == null ? null : plan.Campaign.Name,
                Notes = plan.Notes,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static DateTimeOffset NextExpectedAt(int billingDay)
    {
        var today = DateTimeOffset.UtcNow;
        var candidate = ExpectedAt(today.Year, today.Month, billingDay);
        return candidate.Date < today.Date ? ExpectedAt(today.AddMonths(1).Year, today.AddMonths(1).Month, billingDay) : candidate;
    }

    private static DateTimeOffset ExpectedAt(int year, int month, int billingDay)
    {
        var day = Math.Min(Math.Clamp(billingDay, 1, 28), DateTime.DaysInMonth(year, month));
        return new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero);
    }
}
