using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.DonationPlans.Commands.UpdateDonationPlan;

public record UpdateDonationPlanCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public decimal ExpectedAmount { get; init; }
    public int BillingDay { get; init; }
    public string PreferredPaymentMethod { get; init; } = "Pix";
    public DateTimeOffset StartDateUtc { get; init; }
    public string? Notes { get; init; }
}

public sealed class UpdateDonationPlanCommandHandler : IRequestHandler<UpdateDonationPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public UpdateDonationPlanCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(UpdateDonationPlanCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var plan = await _context.DonationPlans.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (plan is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonationPlan), request.Id.ToString());
        }

        if (plan.Status == DonationPlanStatus.Cancelled)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateDonationPlanCommand.Id), "Planos cancelados nao podem ser editados."),
            ]);
        }

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

        if (plan.DonorId != request.DonorId &&
            await _context.Donations.AsNoTracking().AnyAsync(donation => donation.DonationPlanId == plan.Id, cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateDonationPlanCommand.DonorId), "Nao e possivel trocar o doador de um plano com contribuicoes vinculadas."),
            ]);
        }

        var duplicateActivePlan = await _context.DonationPlans.AsNoTracking().AnyAsync(other =>
            other.Id != plan.Id &&
            other.DonorId == request.DonorId &&
            other.CampaignId == request.CampaignId &&
            other.Status == DonationPlanStatus.Active,
            cancellationToken);

        if (duplicateActivePlan && plan.Status == DonationPlanStatus.Active)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateDonationPlanCommand.CampaignId), "O doador ja possui um plano ativo para este contexto."),
            ]);
        }

        plan.DonorId = request.DonorId;
        plan.CampaignId = request.CampaignId;
        plan.PreferredPaymentMethod = SystemOptionMapper.Parse<PaymentMethod>(request.PreferredPaymentMethod);
        plan.StartDateUtc = request.StartDateUtc;
        plan.Notes = request.Notes?.Trim();
        plan.SetExpectedAmount(request.ExpectedAmount);
        plan.SetBillingDay(request.BillingDay);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = plan.OrganizationId,
            DonorId = plan.DonorId,
            Type = TimelineEntryType.Contact,
            Title = "Plano recorrente atualizado",
            Description = $"Valor esperado: {plan.ExpectedAmount:C}. Dia: {plan.BillingDay}.",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(DonationPlan),
            RelatedEntityId = plan.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
