using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VinculoBackend.Application.Common.Models;

public static partial class ConfigurableOptionCode
{
    public static string FromName(string name)
    {
        var withWordBoundaries = PascalCaseBoundaryRegex().Replace(name.Trim(), "$1-$2");
        var normalized = withWordBoundaries.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        var slug = RepeatedDashesRegex().Replace(builder.ToString(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "opção" : slug;
    }

    public static string CreateUnique(string source, IEnumerable<string> existingCodes)
    {
        var baseCode = FromName(source);
        var code = baseCode;
        var suffix = 2;

        while (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
        {
            code = $"{baseCode}-{suffix}";
            suffix++;
        }

        return code;
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex PascalCaseBoundaryRegex();

    [GeneratedRegex("-+")]
    private static partial Regex RepeatedDashesRegex();
}
