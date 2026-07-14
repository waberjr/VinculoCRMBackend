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
    private readonly TimeProvider _timeProvider;

    public PauseDonationPlanCommandHandler(
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

    public async Task Handle(PauseDonationPlanCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var plan = await _context.DonationPlans.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (plan is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonationPlan), request.Id.ToString());
        }

        var now = _timeProvider.GetUtcNow();
        plan.Pause(now);

        AddTimeline(plan, _context, _user.Id, "Plano recorrente pausado", null, now);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void AddTimeline(DonationPlan plan, IApplicationDbContext context, string? userId, string title, string? description, DateTimeOffset occurredAtUtc)
    {
        context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = plan.OrganizationId,
            DonorId = plan.DonorId,
            Type = TimelineEntryType.Contact,
            Title = title,
            Description = description,
            OccurredAtUtc = occurredAtUtc,
            CreatedByUserId = userId,
            RelatedEntityType = nameof(DonationPlan),
            RelatedEntityId = plan.Id,
        });
    }
}
