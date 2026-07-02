using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.DonationPlans.Commands.PauseDonationPlan;

public record PauseDonationPlanCommand(Guid Id) : IRequest;

public sealed class PauseDonationPlanCommandHandler : IRequestHandler<PauseDonationPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public PauseDonationPlanCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(PauseDonationPlanCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var plan = await _context.DonationPlans.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (plan is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonationPlan), request.Id.ToString());
        }

        if (plan.Status != DonationPlanStatus.Active)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(PauseDonationPlanCommand.Id), "Apenas planos ativos podem ser pausados."),
            ]);
        }

        plan.Status = DonationPlanStatus.Paused;
        plan.PausedAtUtc = DateTimeOffset.UtcNow;

        AddTimeline(plan, _context, _user.Id, "Plano recorrente pausado", null);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void AddTimeline(DonationPlan plan, IApplicationDbContext context, string? userId, string title, string? description)
    {
        context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = plan.OrganizationId,
            DonorId = plan.DonorId,
            Type = TimelineEntryType.Contact,
            Title = title,
            Description = description,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
            RelatedEntityType = nameof(DonationPlan),
            RelatedEntityId = plan.Id,
        });
    }
}
