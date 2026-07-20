using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.UpdateOperationalProductivityGoal;

public sealed record UpdateOperationalProductivityGoalCommand(
    string UserId,
    int? OperationalTaskGoalMonthly,
    int? OperationalSlaHours) : IRequest;

public sealed class UpdateOperationalProductivityGoalCommandHandler : IRequestHandler<UpdateOperationalProductivityGoalCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public UpdateOperationalProductivityGoalCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateOperationalProductivityGoalCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var member = await _context.OrganizationMembers.FirstOrDefaultAsync(entity =>
            entity.OrganizationId == organizationId &&
            entity.UserId == request.UserId &&
            entity.IsActive,
            cancellationToken);
        if (member is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(OrganizationMember), request.UserId);
        }

        var previousGoal = member.OperationalTaskGoalMonthly;
        var previousSla = member.OperationalSlaHours;
        int? newGoal = request.OperationalTaskGoalMonthly is null ? null : Math.Max(0, request.OperationalTaskGoalMonthly.Value);
        int? newSla = request.OperationalSlaHours is null ? null : Math.Max(1, request.OperationalSlaHours.Value);

        member.OperationalTaskGoalMonthly = newGoal;
        member.OperationalSlaHours = newSla;

        if (previousGoal != newGoal || previousSla != newSla)
        {
            _context.OperationalProductivityGoalAuditEntries.Add(new OperationalProductivityGoalAuditEntry
            {
                OrganizationId = organizationId,
                UserId = member.UserId,
                PreviousTaskGoalMonthly = previousGoal,
                NewTaskGoalMonthly = newGoal,
                PreviousSlaHours = previousSla,
                NewSlaHours = newSla,
                ChangedByUserId = _user.Id,
                ChangedAtUtc = _timeProvider.GetUtcNow(),
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
