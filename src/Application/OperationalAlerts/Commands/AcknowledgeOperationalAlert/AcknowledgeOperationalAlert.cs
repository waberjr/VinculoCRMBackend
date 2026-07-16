using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.AcknowledgeOperationalAlert;

public sealed record AcknowledgeOperationalAlertCommand(Guid Id) : IRequest;

public sealed class AcknowledgeOperationalAlertCommandHandler : IRequestHandler<AcknowledgeOperationalAlertCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AcknowledgeOperationalAlertCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AcknowledgeOperationalAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        alert.Acknowledge(_user.Id, _timeProvider.GetUtcNow());
        await _context.SaveChangesAsync(cancellationToken);
    }
}
