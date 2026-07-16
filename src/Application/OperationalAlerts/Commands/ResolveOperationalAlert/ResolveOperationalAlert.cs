using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.ResolveOperationalAlert;

public sealed record ResolveOperationalAlertCommand(Guid Id, string? Note = null) : IRequest;

public sealed class ResolveOperationalAlertCommandHandler : IRequestHandler<ResolveOperationalAlertCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public ResolveOperationalAlertCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ResolveOperationalAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        alert.Resolve(_user.Id, request.Note, _timeProvider.GetUtcNow());
        await _context.SaveChangesAsync(cancellationToken);
    }
}
