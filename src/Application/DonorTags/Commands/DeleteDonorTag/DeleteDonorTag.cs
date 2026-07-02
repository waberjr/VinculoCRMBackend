using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using FluentValidation.Results;

namespace VinculoBackend.Application.DonorTags.Commands.DeleteDonorTag;

public record DeleteDonorTagCommand(Guid Id) : IRequest;

public sealed class DeleteDonorTagCommandHandler : IRequestHandler<DeleteDonorTagCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public DeleteDonorTagCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(DeleteDonorTagCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var entity = await _context.DonorTags.FirstOrDefaultAsync(tag => tag.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonorTags), request.Id.ToString());
        }

        var isInUse = await _context.DonorTagAssignments
            .AsNoTracking()
            .AnyAsync(assignment => assignment.DonorTagId == entity.Id, cancellationToken);

        if (isInUse)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(DeleteDonorTagCommand.Id), "Nao e possivel excluir uma tag relacionada a doadores."),
            ]);
        }

        _context.DonorTags.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
