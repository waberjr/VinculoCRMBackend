using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.RelationshipTasks.Commands.StartRelationshipTask;

public record StartRelationshipTaskCommand(Guid Id) : IRequest;

public sealed class StartRelationshipTaskCommandHandler : IRequestHandler<StartRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public StartRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(StartRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        task.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskStatus", "InProgress", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
