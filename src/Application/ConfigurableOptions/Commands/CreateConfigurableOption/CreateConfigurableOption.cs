using FluentValidation.Results;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.CreateConfigurableOption;

public record CreateConfigurableOptionCommand : IRequest<Guid>
{
    public ConfigurableOptionCategory Category { get; init; }
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
        var category = request.Category.ToString();
        var name = request.Name.Trim();

        var nameAlreadyExists = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .AnyAsync(option =>
                option.OrganizationId == organizationId &&
                option.Category == category &&
                option.Name.ToLower() == name.ToLower(),
                cancellationToken);

        if (nameAlreadyExists)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(request.Name), "Ja existe uma opção com este nome para a categoria.")
            ]);
        }

        var code = await CreateUniqueCodeAsync(organizationId, category, name, cancellationToken);

        var entity = new ConfigurableOption
        {
            OrganizationId = organizationId,
            Category = category,
            Code = code,
            Name = name,
            Description = request.Description?.Trim(),
            Color = request.Color?.Trim(),
            SortOrder = request.SortOrder,
        };

        _context.ConfigurableOptions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<string> CreateUniqueCodeAsync(Guid organizationId, string category, string name, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .Where(option => option.OrganizationId == organizationId && option.Category == category)
            .Select(option => option.Code)
            .ToListAsync(cancellationToken);

        return ConfigurableOptionCode.CreateUnique(name, existingCodes);
    }
}
