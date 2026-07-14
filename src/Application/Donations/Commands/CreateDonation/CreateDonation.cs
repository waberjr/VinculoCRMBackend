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
    private readonly TimeProvider _timeProvider;

    public CreateDonationCommandHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateDonationCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        string? campaignName = null;
        if (request.CampaignId is not null)
        {
            campaignName = await _context.Campaigns
                .AsNoTracking()
                .Where(campaign => campaign.Id == request.CampaignId)
                .Select(campaign => campaign.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (campaignName is null)
            {
                throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.CampaignId.Value.ToString());
            }
        }

        string? projectName = null;
        if (request.ProjectId is not null)
        {
            projectName = await _context.Projects
                .AsNoTracking()
                .Where(project => project.Id == request.ProjectId)
                .Select(project => project.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (projectName is null)
            {
                throw new Common.Exceptions.NotFoundException(nameof(Project), request.ProjectId.Value.ToString());
            }
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

        var donationStatus = SystemOptionMapper.Parse<DonationStatus>(request.Status);
        var donation = Donation.Create(
            organizationId,
            request.DonorId,
            request.CampaignId,
            request.DonationPlanId,
            request.Amount,
            SystemOptionMapper.Parse<DonationType>(request.Type),
            donationStatus,
            SystemOptionMapper.Parse<PaymentMethod>(request.PaymentMethod),
            request.ExpectedAtUtc,
            request.PaidAtUtc,
            request.Reference,
            request.ExternalPaymentId,
            request.Notes,
            _user.Id);

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
            Title = donationStatus == DonationStatus.Confirmed
                ? "Contribuição registrada como confirmada"
                : "Contribuição registrada",
            Description = DonationTimelineDescription(donation.Amount, campaignName, projectName, request.DonationPlanId is not null),
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return donation.Id;
    }

    private static string DonationTimelineDescription(
        decimal amount,
        string? campaignName,
        string? projectName,
        bool hasDonationPlan)
    {
        var parts = new List<string> { $"Valor: {amount:C}." };

        if (!string.IsNullOrWhiteSpace(campaignName))
        {
            parts.Add($"Campanha: {campaignName}.");
        }

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            parts.Add($"Projeto/destinacao: {projectName}.");
        }

        if (hasDonationPlan)
        {
            parts.Add("Vinculada a recorrencia.");
        }

        return string.Join(" ", parts);
    }
}
