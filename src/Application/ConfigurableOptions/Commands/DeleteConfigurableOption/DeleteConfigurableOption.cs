using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.DeleteConfigurableOption;

public record DeleteConfigurableOptionCommand(Guid Id) : IRequest;

public sealed class DeleteConfigurableOptionCommandHandler : IRequestHandler<DeleteConfigurableOptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public DeleteConfigurableOptionCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(DeleteConfigurableOptionCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var entity = await _context.ConfigurableOptions.FirstOrDefaultAsync(option => option.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ConfigurableOptions), request.Id.ToString());
        }

        _context.ConfigurableOptions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
