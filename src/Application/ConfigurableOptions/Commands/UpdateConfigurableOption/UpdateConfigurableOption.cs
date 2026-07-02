using FluentValidation.Results;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.UpdateConfigurableOption;

public record UpdateConfigurableOptionCommand : IRequest
{
    public Guid Id { get; init; }
    public ConfigurableOptionCategory Category { get; init; }
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
        var organizationId = RequiredOrganization.From(_organizationContext);

        var entity = await _context.ConfigurableOptions.FirstOrDefaultAsync(option => option.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ConfigurableOptions), request.Id.ToString());
        }

        var category = request.Category.ToString();
        var name = request.Name.Trim();
        var nameAlreadyExists = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .AnyAsync(option =>
                option.Id != request.Id &&
                option.OrganizationId == organizationId &&
                option.Category == category &&
                option.Name.ToLower() == name.ToLower(),
                cancellationToken);

        if (nameAlreadyExists)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(request.Name), "Ja existe uma opcao com este nome para a categoria.")
            ]);
        }

        entity.Category = category;
        entity.Code = await CreateUniqueCodeAsync(organizationId, category, name, request.Id, cancellationToken);
        entity.Name = name;
        entity.Description = request.Description?.Trim();
        entity.Color = request.Color?.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> CreateUniqueCodeAsync(Guid organizationId, string category, string name, Guid currentOptionId, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .Where(option => option.Id != currentOptionId && option.OrganizationId == organizationId && option.Category == category)
            .Select(option => option.Code)
            .ToListAsync(cancellationToken);

        return ConfigurableOptionCode.CreateUnique(name, existingCodes);
    }
}
