using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Donations.Commands.CreateDonation;

public record CreateDonationCommand : IRequest<Guid>
{
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? ProjectId { get; init; }
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

        if (request.CampaignId is not null &&
            !await _context.Campaigns.AsNoTracking().AnyAsync(campaign => campaign.Id == request.CampaignId, cancellationToken))
        {
            throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.CampaignId.Value.ToString());
        }

        if (request.ProjectId is not null &&
            !await _context.Projects.AsNoTracking().AnyAsync(project => project.Id == request.ProjectId, cancellationToken))
        {
            throw new Common.Exceptions.NotFoundException(nameof(Project), request.ProjectId.Value.ToString());
        }

        if (request.DonationPlanId is not null)
        {
            var planBelongsToDonor = await _context.DonationPlans
                .AsNoTracking()
                .AnyAsync(plan => plan.Id == request.DonationPlanId && plan.DonorId == request.DonorId, cancellationToken);

            if (!planBelongsToDonor)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(CreateDonationCommand.DonationPlanId), "O plano recorrente informado não pertence ao doador."),
                ]);
            }
        }

        var donation = new Donation
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            CampaignId = request.CampaignId,
            DonationPlanId = request.DonationPlanId,
            Type = SystemOptionMapper.Parse<DonationType>(request.Type),
            Status = SystemOptionMapper.Parse<DonationStatus>(request.Status),
            PaymentMethod = SystemOptionMapper.Parse<PaymentMethod>(request.PaymentMethod),
            ExpectedAtUtc = request.ExpectedAtUtc,
            PaidAtUtc = request.PaidAtUtc,
            Reference = request.Reference?.Trim(),
            ExternalPaymentId = request.ExternalPaymentId?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedByUserId = _user.Id,
        };

        donation.SetAmount(request.Amount);

        _context.Donations.Add(donation);
        if (request.ProjectId is not null)
        {
            _context.DonationProjects.Add(new DonationProject
            {
                OrganizationId = organizationId,
                DonationId = donation.Id,
                ProjectId = request.ProjectId.Value,
            });
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donation.DonorId,
            Type = TimelineEntryType.Donation,
            Title = SystemOptionMapper.Parse<DonationStatus>(request.Status) == DonationStatus.Confirmed
                ? "Contribuição registrada como confirmada"
                : "Contribuição registrada",
            Description = $"Valor: {donation.Amount:C}.",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return donation.Id;
    }
}
