using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.DonationPlans.Commands.CreateDonationPlan;

public record CreateDonationPlanCommand : IRequest<Guid>
{
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public decimal ExpectedAmount { get; init; }
    public int BillingDay { get; init; }
    public string PreferredPaymentMethod { get; init; } = "Pix";
    public DateTimeOffset StartDateUtc { get; init; }
    public string? Notes { get; init; }
}

public sealed class CreateDonationPlanCommandHandler : IRequestHandler<CreateDonationPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateDonationPlanCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDonationPlanCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        var hasActivePlanForCampaign = await _context.DonationPlans.AsNoTracking().AnyAsync(plan =>
            plan.DonorId == request.DonorId &&
            plan.CampaignId == request.CampaignId &&
            plan.Status == DonationPlanStatus.Active,
            cancellationToken);

        if (hasActivePlanForCampaign)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(
                    nameof(request.CampaignId),
                    request.CampaignId is null
                        ? "O doador ja possui um plano geral ativo."
                        : "O doador ja possui um plano ativo para esta campanha.")
            ]);
        }

        var plan = new DonationPlan
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            CampaignId = request.CampaignId,
            AssignedUserId = _user.Id,
            PreferredPaymentMethod = SystemOptionMapper.Parse<PaymentMethod>(request.PreferredPaymentMethod),
            StartDateUtc = request.StartDateUtc,
            Status = DonationPlanStatus.Active,
            Notes = request.Notes?.Trim(),
        };

        plan.SetExpectedAmount(request.ExpectedAmount);
        plan.SetBillingDay(request.BillingDay);

        _context.DonationPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
