using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CancelRelationshipTask;

public record CancelRelationshipTaskCommand(Guid Id) : IRequest;

public sealed class CancelRelationshipTaskCommandHandler : IRequestHandler<CancelRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CancelRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(CancelRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        if (task.Status is not (RelationshipTaskStatus.Open or RelationshipTaskStatus.InProgress))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(CancelRelationshipTaskCommand.Id), "Apenas tarefas abertas ou em andamento podem ser canceladas."),
            ]);
        }

        task.Status = RelationshipTaskStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
