using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.DonorTags.Commands.UpdateDonorTag;

public record UpdateDonorTagCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateDonorTagCommandHandler : IRequestHandler<UpdateDonorTagCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateDonorTagCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateDonorTagCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var entity = await _context.DonorTags.FirstOrDefaultAsync(tag => tag.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(DonorTags), request.Id.ToString());
        }

        entity.Update(request.Name, request.Description, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
