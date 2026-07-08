using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;

public record GetReceiptPrintQuery(Guid Id) : IRequest<ReceiptPrintDto?>;

public sealed class GetReceiptPrintQueryHandler : IRequestHandler<GetReceiptPrintQuery, ReceiptPrintDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetReceiptPrintQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<ReceiptPrintDto?> Handle(GetReceiptPrintQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        return await _context.Receipts
            .AsNoTracking()
            .Where(receipt =>
                receipt.Id == request.Id &&
                (receipt.Status == ReceiptStatus.Issued || receipt.Status == ReceiptStatus.Reissued) &&
                receipt.IssuedAtUtc != null &&
                receipt.Donation.PaidAtUtc != null)
            .Select(receipt => new ReceiptPrintDto
            {
                Id = receipt.Id,
                Number = receipt.Number,
                OrganizationName = _context.Organizations
                    .Where(organization => organization.Id == organizationId)
                    .Select(organization => organization.Name)
                    .FirstOrDefault() ?? "Organizacao",
                OrganizationDocument = _context.Organizations
                    .Where(organization => organization.Id == organizationId)
                    .Select(organization => organization.Document)
                    .FirstOrDefault(),
                OrganizationLogoUrl = _context.Organizations
                    .Where(organization => organization.Id == organizationId)
                    .Select(organization => organization.LogoUrl)
                    .FirstOrDefault(),
                DonorName = receipt.Donor.FullName,
                DonorDocument = receipt.Donor.Document,
                CampaignName = receipt.Donation.Campaign == null ? null : receipt.Donation.Campaign.Name,
                ProjectName = _context.DonationProjects
                    .Where(projectLink => projectLink.DonationId == receipt.DonationId)
                    .Select(projectLink => projectLink.Project.Name)
                    .FirstOrDefault(),
                Amount = receipt.Amount,
                PaidAtUtc = receipt.Donation.PaidAtUtc!.Value,
                IssuedAtUtc = receipt.IssuedAtUtc!.Value,
                DonationReference = receipt.Donation.Reference ?? receipt.DonationId.ToString(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
