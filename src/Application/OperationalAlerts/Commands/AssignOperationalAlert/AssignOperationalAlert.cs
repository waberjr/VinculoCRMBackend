using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.AssignOperationalAlert;

public sealed record AssignOperationalAlertCommand(Guid Id, string? AssignedUserId, DateTimeOffset? DueAtUtc) : IRequest;

public sealed class AssignOperationalAlertCommandHandler : IRequestHandler<AssignOperationalAlertCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignOperationalAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignOperationalAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        alert.Assign(request.AssignedUserId, request.DueAtUtc);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
