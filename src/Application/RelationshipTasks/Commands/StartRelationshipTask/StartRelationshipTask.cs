using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

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

        var task = await _context.RelationshipTasks
            .Include(entity => entity.StatusOption)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        if (task.StatusOption.Code != "open")
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(StartRelationshipTaskCommand.Id), "Apenas tarefas abertas podem ser iniciadas."),
            ]);
        }

        task.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskStatus", "InProgress", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
