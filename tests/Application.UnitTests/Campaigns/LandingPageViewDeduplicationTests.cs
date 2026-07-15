using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class LandingPageViewDeduplicationTests
{
    [Test]
    public void WindowShouldGroupViewsByHour()
    {
        var viewedAtUtc = new DateTimeOffset(2026, 7, 14, 15, 43, 12, TimeSpan.Zero);

        var window = LandingPageViewDeduplication.Window(viewedAtUtc);

        window.ShouldBe(new DateTimeOffset(2026, 7, 14, 15, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public void FingerprintShouldBeStableForSameVisitor()
    {
        var targetId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var first = LandingPageViewDeduplication.Fingerprint("campaign", targetId, "crm", "127.0.0.1", "browser");
        var second = LandingPageViewDeduplication.Fingerprint("campaign", targetId, "crm", "127.0.0.1", "browser");

        first.ShouldBe(second);
        first.Length.ShouldBe(64);
    }

    [Test]
    public void FingerprintShouldChangeWhenSourceChanges()
    {
        var targetId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var crm = LandingPageViewDeduplication.Fingerprint("campaign", targetId, "crm", "127.0.0.1", "browser");
        var qrCode = LandingPageViewDeduplication.Fingerprint("campaign", targetId, "qrcode", "127.0.0.1", "browser");

        crm.ShouldNotBe(qrCode);
    }
}
