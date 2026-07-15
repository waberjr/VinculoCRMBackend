using System.Security.Cryptography;
using System.Text;

namespace VinculoBackend.Application.Campaigns.Services;

public static class LandingPageViewDeduplication
{
    public static DateTimeOffset Window(DateTimeOffset viewedAtUtc)
    {
        var utc = viewedAtUtc.UtcDateTime;
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
    }

    public static string Fingerprint(string targetType, Guid targetId, string source, string ipAddress, string userAgent)
    {
        var input = $"{targetType}|{targetId:N}|{source}|{ipAddress}|{userAgent}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
