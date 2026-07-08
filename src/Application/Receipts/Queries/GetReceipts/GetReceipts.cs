using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Receipts.Models;

namespace VinculoBackend.Application.Receipts.Queries.GetReceipts;

public record GetReceiptsQuery : IRequest<PaginatedResult<ReceiptListItemDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public sealed class GetReceiptsQueryHandler : IRequestHandler<GetReceiptsQuery, PaginatedResult<ReceiptListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetReceiptsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<ReceiptListItemDto>> Handle(GetReceiptsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.Receipts
            .AsNoTracking()
            .OrderByDescending(receipt => receipt.IssuedAtUtc ?? receipt.Created)
            .Select(receipt => new ReceiptListItemDto
            {
                Id = receipt.Id,
                DonationId = receipt.DonationId,
                Number = receipt.Number,
                DonorName = receipt.Donor.FullName,
                CampaignName = receipt.Donation.Campaign == null ? null : receipt.Donation.Campaign.Name,
                ProjectName = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == receipt.DonationId)
                    .Select(projectLink => projectLink.Project.Name)
                    .FirstOrDefault(),
                DonationReference = receipt.Donation.Reference ?? receipt.DonationId.ToString(),
                Amount = receipt.Amount,
                Status = receipt.Status,
                IssuedAtUtc = receipt.IssuedAtUtc,
                FileUrl = receipt.FileUrl,
            });

        return await PaginatedResult<ReceiptListItemDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
