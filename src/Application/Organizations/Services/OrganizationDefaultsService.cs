using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Organizations.Services;

public sealed class OrganizationDefaultsService : IOrganizationDefaultsService
{
    private readonly IApplicationDbContext _context;

    public OrganizationDefaultsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureDefaultsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var existingOptions = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .Where(option => option.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        foreach (var option in DefaultOptions())
        {
            var category = option.Category.ToString();
            var existingCodes = existingOptions
                .Where(existing => existing.Category == category)
                .Select(existing => existing.Code)
                .ToList();
            var baseCode = ConfigurableOptionCode.FromName(option.Code);
            var existingSystemOption = existingOptions.FirstOrDefault(existing =>
                existing.Category == category &&
                string.Equals(existing.Code, baseCode, StringComparison.OrdinalIgnoreCase));
            if (existingSystemOption is not null)
            {
                existingSystemOption.IsSystem = false;
                continue;
            }

            var existingLegacyOption = existingOptions.FirstOrDefault(existing =>
                existing.Category == category &&
                string.Equals(existing.Code, option.Code, StringComparison.OrdinalIgnoreCase));

            if (existingLegacyOption is not null)
            {
                existingLegacyOption.Code = baseCode;
                existingLegacyOption.IsSystem = false;
                continue;
            }

            var code = ConfigurableOptionCode.CreateUnique(option.Code, existingCodes);

            if (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _context.ConfigurableOptions.Add(new ConfigurableOption
            {
                OrganizationId = organizationId,
                Category = category,
                Code = code,
                Name = option.Name,
                Color = option.Color,
                SortOrder = option.SortOrder,
                IsSystem = false,
                IsActive = true,
            });

            existingOptions.Add(new ConfigurableOption
            {
                OrganizationId = organizationId,
                Category = category,
                Code = code,
            });
        }

        var existingTagNames = await _context.DonorTags
            .IgnoreQueryFilters()
            .Where(tag => tag.OrganizationId == organizationId)
            .Select(tag => tag.Name.ToLower())
            .ToListAsync(cancellationToken);

        foreach (var tag in DefaultTags())
        {
            if (existingTagNames.Contains(tag.Name.ToLower()))
            {
                continue;
            }

            _context.DonorTags.Add(new DonorTag
            {
                OrganizationId = organizationId,
                Name = tag.Name,
                Description = tag.Description,
                IsActive = true,
            });
        }
    }

    private static IReadOnlyCollection<DefaultOption> DefaultOptions() =>
    [
        Option(ConfigurableOptionCategory.RelationshipProfile, "New", "Novo", 1),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Recurring", "Recorrente", 2),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Major", "Grande doador", 3),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Lapsed", "Inativo", 4),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Reactivated", "Reativado", 5),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Prospect", "Prospect", 6),
        Option(ConfigurableOptionCategory.DonorSource, "Manual", "Manual", 1),
        Option(ConfigurableOptionCategory.DonorSource, "Referral", "Indicacao", 2),
        Option(ConfigurableOptionCategory.DonorSource, "Phone", "Telefone", 3),
        Option(ConfigurableOptionCategory.DonorSource, "WhatsApp", "WhatsApp", 4),
        Option(ConfigurableOptionCategory.DonorSource, "Email", "E-mail", 5),
        Option(ConfigurableOptionCategory.DonorSource, "SocialMedia", "Redes sociais", 6),
        Option(ConfigurableOptionCategory.DonorSource, "Website", "Website", 7),
        Option(ConfigurableOptionCategory.DonorSource, "Event", "Evento", 8),
        Option(ConfigurableOptionCategory.DonorSource, "Import", "Importacao", 9),
        Option(ConfigurableOptionCategory.DonorSource, "Other", "Outro", 10),
        Option(ConfigurableOptionCategory.ContactChannel, "Phone", "Telefone", 1),
        Option(ConfigurableOptionCategory.ContactChannel, "WhatsApp", "WhatsApp", 2),
        Option(ConfigurableOptionCategory.ContactChannel, "Email", "E-mail", 3),
        Option(ConfigurableOptionCategory.ContactChannel, "Other", "Outro", 4),
    ];

    private static IReadOnlyCollection<DefaultTag> DefaultTags() =>
    [
        new("Recorrente", "Doador com relacionamento recorrente."),
        new("Alto valor", "Doador com histórico relevante de contribuições."),
    ];

    private static DefaultOption Option(ConfigurableOptionCategory category, string code, string name, int sortOrder, string? color = null) =>
        new(category, code, name, sortOrder, color);

    private sealed record DefaultOption(ConfigurableOptionCategory Category, string Code, string Name, int SortOrder, string? Color);

    private sealed record DefaultTag(string Name, string Description);
}
