using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.UpdateConfigurableOption;

public record UpdateConfigurableOptionCommand : IRequest
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateConfigurableOptionCommandHandler : IRequestHandler<UpdateConfigurableOptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateConfigurableOptionCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateConfigurableOptionCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var entity = await _context.ConfigurableOptions.FirstOrDefaultAsync(option => option.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ConfigurableOptions), request.Id.ToString());
        }

        entity.Category = request.Category.Trim();
        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.Color = request.Color?.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
