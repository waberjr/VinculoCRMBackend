using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.RelationshipTasks.Commands.ReopenRelationshipTask;

public record ReopenRelationshipTaskCommand(Guid Id) : IRequest;

public sealed class ReopenRelationshipTaskCommandHandler : IRequestHandler<ReopenRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public ReopenRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(ReopenRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        task.Reopen();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
