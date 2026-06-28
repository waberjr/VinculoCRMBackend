using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donations.Commands.CreateDonation;

public record CreateDonationCommand : IRequest<Guid>
{
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? DonationPlanId { get; init; }
    public decimal Amount { get; init; }
    public Guid TypeOptionId { get; init; }
    public Guid StatusOptionId { get; init; }
    public Guid PaymentMethodOptionId { get; init; }
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

        var donation = new Donation
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            CampaignId = request.CampaignId,
            DonationPlanId = request.DonationPlanId,
            TypeOptionId = request.TypeOptionId,
            StatusOptionId = request.StatusOptionId,
            PaymentMethodOptionId = request.PaymentMethodOptionId,
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
