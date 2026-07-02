using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.DonationPlans.Commands.ResumeDonationPlan;

public record ResumeDonationPlanCommand(Guid Id) : IRequest;

public sealed class ResumeDonationPlanCommandHandler : IRequestHandler<ResumeDonationPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public ResumeDonationPlanCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(ResumeDonationPlanCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var plan = await _context.DonationPlans.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (plan is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonationPlan), request.Id.ToString());
        }

        if (plan.Status != DonationPlanStatus.Paused)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(ResumeDonationPlanCommand.Id), "Apenas planos pausados podem ser retomados."),
            ]);
        }

        var hasActivePlanForCampaign = await _context.DonationPlans.AsNoTracking().AnyAsync(other =>
            other.Id != plan.Id &&
            other.DonorId == plan.DonorId &&
            other.CampaignId == plan.CampaignId &&
            other.Status == DonationPlanStatus.Active,
            cancellationToken);

        if (hasActivePlanForCampaign)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(ResumeDonationPlanCommand.Id), "O doador ja possui um plano ativo para este contexto."),
            ]);
        }

        plan.Status = DonationPlanStatus.Active;
        plan.PausedAtUtc = null;

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = plan.OrganizationId,
            DonorId = plan.DonorId,
            Type = TimelineEntryType.Contact,
            Title = "Plano recorrente retomado",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(DonationPlan),
            RelatedEntityId = plan.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
