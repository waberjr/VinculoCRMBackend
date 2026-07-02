using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Common.Models;

public static class OptionLookup
{
    public static Task<Guid> RequiredIdAsync(
        IApplicationDbContext context,
        ConfigurableOptionCategory category,
        string code,
        CancellationToken cancellationToken)
    {
        return RequiredIdAsync(context, category.ToString(), code, cancellationToken);
    }

    public static async Task<Guid> RequiredIdAsync(
        IApplicationDbContext context,
        string category,
        string code,
        CancellationToken cancellationToken)
    {
        var normalizedCode = ConfigurableOptionCode.FromName(code);
        var optionId = await context.ConfigurableOptions
            .AsNoTracking()
            .Where(option => option.Category == category && option.Code == normalizedCode && option.IsActive)
            .Select(option => (Guid?)option.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (optionId is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.ConfigurableOption), $"{category}:{code}");
        }

        return optionId.Value;
    }
}
