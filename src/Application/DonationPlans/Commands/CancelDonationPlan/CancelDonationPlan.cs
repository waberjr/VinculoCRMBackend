using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.DonationPlans.Commands.CancelDonationPlan;

public record CancelDonationPlanCommand(Guid Id, string Reason) : IRequest;

public sealed class CancelDonationPlanCommandHandler : IRequestHandler<CancelDonationPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CancelDonationPlanCommandHandler(
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

    public async Task Handle(CancelDonationPlanCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var plan = await _context.DonationPlans.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (plan is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonationPlan), request.Id.ToString());
        }

        var now = _timeProvider.GetUtcNow();
        plan.Cancel(request.Reason, now);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = plan.OrganizationId,
            DonorId = plan.DonorId,
            Type = TimelineEntryType.Contact,
            Title = "Plano recorrente cancelado",
            Description = plan.CancellationReason,
            OccurredAtUtc = now,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(DonationPlan),
            RelatedEntityId = plan.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
