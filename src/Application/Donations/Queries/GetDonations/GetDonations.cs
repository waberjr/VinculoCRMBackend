using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donations.Models;

namespace VinculoBackend.Application.Donations.Queries.GetDonations;

public record GetDonationsQuery : IRequest<PaginatedResult<DonationListItemDto>>
{
    public string? Search { get; init; }
    public Guid? DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? StatusOptionId { get; init; }
    public Guid? PaymentMethodOptionId { get; init; }
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
                (donation.Reference != null && donation.Reference.ToLower().Contains(search)));
        }

        if (request.DonorId is not null) query = query.Where(donation => donation.DonorId == request.DonorId);
        if (request.CampaignId is not null) query = query.Where(donation => donation.CampaignId == request.CampaignId);
        if (request.StatusOptionId is not null) query = query.Where(donation => donation.StatusOptionId == request.StatusOptionId);
        if (request.PaymentMethodOptionId is not null) query = query.Where(donation => donation.PaymentMethodOptionId == request.PaymentMethodOptionId);
        if (request.FromUtc is not null) query = query.Where(donation => donation.ExpectedAtUtc >= request.FromUtc || donation.PaidAtUtc >= request.FromUtc);
        if (request.ToUtc is not null) query = query.Where(donation => donation.ExpectedAtUtc <= request.ToUtc || donation.PaidAtUtc <= request.ToUtc);

        var projected = query
            .OrderByDescending(donation => donation.PaidAtUtc ?? donation.ExpectedAtUtc ?? donation.Created)
            .Select(donation => new DonationListItemDto
            {
                Id = donation.Id,
                DonorId = donation.DonorId,
                DonorName = donation.Donor.FullName,
                CampaignId = donation.CampaignId,
                CampaignName = donation.Campaign == null ? null : donation.Campaign.Name,
                Amount = donation.Amount,
                ExpectedAtUtc = donation.ExpectedAtUtc,
                PaidAtUtc = donation.PaidAtUtc,
                Reference = donation.Reference,
                Type = new OptionDto { Id = donation.TypeOption.Id, Category = donation.TypeOption.Category, Code = donation.TypeOption.Code, Name = donation.TypeOption.Name, Color = donation.TypeOption.Color, SortOrder = donation.TypeOption.SortOrder, IsSystem = donation.TypeOption.IsSystem, IsActive = donation.TypeOption.IsActive },
                Status = new OptionDto { Id = donation.StatusOption.Id, Category = donation.StatusOption.Category, Code = donation.StatusOption.Code, Name = donation.StatusOption.Name, Color = donation.StatusOption.Color, SortOrder = donation.StatusOption.SortOrder, IsSystem = donation.StatusOption.IsSystem, IsActive = donation.StatusOption.IsActive },
                PaymentMethod = new OptionDto { Id = donation.PaymentMethodOption.Id, Category = donation.PaymentMethodOption.Category, Code = donation.PaymentMethodOption.Code, Name = donation.PaymentMethodOption.Name, Color = donation.PaymentMethodOption.Color, SortOrder = donation.PaymentMethodOption.SortOrder, IsSystem = donation.PaymentMethodOption.IsSystem, IsActive = donation.PaymentMethodOption.IsActive },
            });

        return await PaginatedResult<DonationListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
