using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CompleteRelationshipTask;

public record CompleteRelationshipTaskCommand(
    Guid Id,
    Guid CompletedStatusOptionId,
    Guid? ContactOutcomeOptionId,
    DateTimeOffset CompletedAtUtc,
    string? CompletionNote) : IRequest;

public sealed class CompleteRelationshipTaskCommandHandler : IRequestHandler<CompleteRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CompleteRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(CompleteRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        task.StatusOptionId = request.CompletedStatusOptionId;
        task.ContactOutcomeOptionId = request.ContactOutcomeOptionId;
        task.CompletedAtUtc = request.CompletedAtUtc;
        task.CompletionNote = request.CompletionNote?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
