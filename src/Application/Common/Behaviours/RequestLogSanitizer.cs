using System.Collections;
using System.Reflection;

namespace VinculoBackend.Application.Common.Behaviours;

public static class RequestLogSanitizer
{
    private const string Redacted = "[Redacted]";

    private static readonly string[] SensitiveNameParts =
    [
        "password",
        "token",
        "secret",
        "document",
        "cpf",
        "cnpj",
        "email",
        "phone",
        "whatsapp",
        "address",
        "postal",
        "amount",
        "value",
        "price",
        "fee",
        "reference",
        "externalpayment",
        "note",
        "reason",
        "description",
        "search",
    ];

    private static readonly string[] SafeStringNameParts =
    [
        "id",
        "status",
        "type",
        "priority",
        "method",
        "role",
        "channel",
        "category",
        "segment",
    ];

    public static IReadOnlyDictionary<string, object?> Sanitize<TRequest>(TRequest request)
        where TRequest : notnull
    {
        var values = new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).Name,
        };

        foreach (var property in typeof(TRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            values[property.Name] = SanitizeValue(property.Name, property.GetValue(request));
        }

        return values;
    }

    private static object? SanitizeValue(string propertyName, object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (IsSensitive(propertyName))
        {
            return Redacted;
        }

        if (value is string text)
        {
            return IsSafeStringProperty(propertyName) ? text : Redacted;
        }

        if (value is decimal or double or float)
        {
            return Redacted;
        }

        if (value is DateTime or DateTimeOffset or DateOnly or TimeOnly or Guid or bool || value.GetType().IsEnum)
        {
            return value;
        }

        if (value is int or long or short or byte)
        {
            return value;
        }

        if (value is IEnumerable enumerable)
        {
            return new
            {
                Count = enumerable.Cast<object?>().Count(),
            };
        }

        return Redacted;
    }

    private static bool IsSensitive(string propertyName)
    {
        var normalized = propertyName.ToLowerInvariant();
        return SensitiveNameParts.Any(normalized.Contains);
    }

    private static bool IsSafeStringProperty(string propertyName)
    {
        var normalized = propertyName.ToLowerInvariant();
        return SafeStringNameParts.Any(normalized.Contains);
    }
}
