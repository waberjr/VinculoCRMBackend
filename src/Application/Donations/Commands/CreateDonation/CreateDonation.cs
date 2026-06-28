using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donations.Commands.CreateDonation;

public record CreateDonationCommand : IRequest<Guid>
{
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? DonationPlanId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = "OneTime";
    public string Status { get; init; } = "Pending";
    public string PaymentMethod { get; init; } = "Pix";
    public DateTimeOffset? ExpectedAtUtc { get; init; }
    public DateTimeOffset? PaidAtUtc { get; init; }
    public string? Reference { get; init; }
    public string? ExternalPaymentId { get; init; }
    public string? Notes { get; init; }
}

public sealed class CreateDonationCommandHandler : IRequestHandler<CreateDonationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateDonationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDonationCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        var donation = new Donation
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            CampaignId = request.CampaignId,
            DonationPlanId = request.DonationPlanId,
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "DonationType", request.Type, cancellationToken),
            StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonationStatus", request.Status, cancellationToken),
            PaymentMethodOptionId = await OptionLookup.RequiredIdAsync(_context, "PaymentMethod", request.PaymentMethod, cancellationToken),
            ExpectedAtUtc = request.ExpectedAtUtc,
            PaidAtUtc = request.PaidAtUtc,
            Reference = request.Reference?.Trim(),
            ExternalPaymentId = request.ExternalPaymentId?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedByUserId = _user.Id,
        };

        donation.SetAmount(request.Amount);

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync(cancellationToken);

        return donation.Id;
    }
}
