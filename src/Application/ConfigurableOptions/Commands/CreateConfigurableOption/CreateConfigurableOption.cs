using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.CreateConfigurableOption;

public record CreateConfigurableOptionCommand : IRequest<Guid>
{
    public string Category { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CreateConfigurableOptionCommandHandler : IRequestHandler<CreateConfigurableOptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CreateConfigurableOptionCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateConfigurableOptionCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var entity = new ConfigurableOption
        {
            OrganizationId = organizationId,
            Category = request.Category.Trim(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Color = request.Color?.Trim(),
            SortOrder = request.SortOrder,
        };

        _context.ConfigurableOptions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
