using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Common.Models;

public static class OptionLookup
{
    public static async Task<Guid> RequiredIdAsync(
        IApplicationDbContext context,
        string category,
        string code,
        CancellationToken cancellationToken)
    {
        var optionId = await context.ConfigurableOptions
            .AsNoTracking()
            .Where(option => option.Category == category && option.Code == code && option.IsActive)
            .Select(option => (Guid?)option.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (optionId is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.ConfigurableOption), $"{category}:{code}");
        }

        return optionId.Value;
    }
}
