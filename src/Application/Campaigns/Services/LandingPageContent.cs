using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using VinculoBackend.Application.Campaigns.Models;

namespace VinculoBackend.Application.Campaigns.Services;

public static class LandingPageContent
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyCollection<LandingPageCustomFieldDto> ParseFields(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyCollection<LandingPageCustomFieldDto>>(json, SerializerOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string SerializeFields(IEnumerable<LandingPageCustomFieldDto> fields)
    {
        var normalized = fields
            .Where(field => !string.IsNullOrWhiteSpace(field.Label))
            .Select(field => new LandingPageCustomFieldDto
            {
                Key = Key(field.Key, field.Label),
                Label = field.Label.Trim(),
                Required = field.Required,
            })
            .Take(10)
            .ToArray();

        return normalized.Length == 0 ? string.Empty : JsonSerializer.Serialize(normalized, SerializerOptions);
    }

    public static string PublicUrl(string targetType, Guid targetId) => $"/captacao/{targetType}/{targetId}";

    public static string TrackableUrl(string targetType, Guid targetId, string source = "crm")
        => $"{PublicUrl(targetType, targetId)}?source={Uri.EscapeDataString(source)}&utm_source={Uri.EscapeDataString(source)}&utm_medium=crm&utm_campaign={Uri.EscapeDataString(targetType)}-{targetId:N}";

    public static string SourceFromTimeline(string description)
    {
        return Extract(description, "Fonte") ?? Extract(description, "utm_source") ?? "landing";
    }

    public static string? UtmSourceFromTimeline(string description) => Extract(description, "utm_source");

    private static string Key(string key, string label)
    {
        var source = string.IsNullOrWhiteSpace(key) ? label : key;
        var normalized = Regex.Replace(source.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
        return string.IsNullOrWhiteSpace(normalized) ? Guid.NewGuid().ToString("N")[..8] : normalized;
    }

    private static string? Extract(string value, string label)
    {
        var marker = $"{label}:";
        var start = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += marker.Length;
        var end = value.IndexOf('|', start);
        var extracted = end < 0 ? value[start..] : value[start..end];
        return WebUtility.HtmlDecode(extracted).Trim().Trim('.');
    }
}
