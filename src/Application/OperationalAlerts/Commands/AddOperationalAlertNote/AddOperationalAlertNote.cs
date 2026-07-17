using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.OperationalAlerts.Services;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Application.OperationalAlerts.Commands.AddOperationalAlertNote;

public sealed record AddOperationalAlertNoteCommand(Guid Id, string Note) : IRequest;

public sealed class AddOperationalAlertNoteCommandHandler : IRequestHandler<AddOperationalAlertNoteCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AddOperationalAlertNoteCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AddOperationalAlertNoteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            throw new DomainValidationException("Informe a nota do alerta.");
        }

        var alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        var now = _timeProvider.GetUtcNow();
        _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
            alert,
            "Note",
            "Nota registrada",
            request.Note,
            now,
            _user.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
