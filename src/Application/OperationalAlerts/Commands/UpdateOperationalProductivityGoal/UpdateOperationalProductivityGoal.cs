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

    public UpdateOperationalProductivityGoalCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
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

        member.OperationalTaskGoalMonthly = request.OperationalTaskGoalMonthly is null ? null : Math.Max(0, request.OperationalTaskGoalMonthly.Value);
        member.OperationalSlaHours = request.OperationalSlaHours is null ? null : Math.Max(1, request.OperationalSlaHours.Value);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
