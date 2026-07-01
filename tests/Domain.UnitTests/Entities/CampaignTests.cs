using VinculoBackend.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class CampaignTests
{
    [Test]
    public void SetPeriodShouldStoreValidPeriod()
    {
        var campaign = new Campaign();
        var startDateUtc = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var endDateUtc = new DateTimeOffset(2026, 7, 2, 0, 0, 0, TimeSpan.Zero);

        campaign.SetPeriod(startDateUtc, endDateUtc);

        campaign.StartDateUtc.ShouldBe(startDateUtc);
        campaign.EndDateUtc.ShouldBe(endDateUtc);
    }

    [Test]
    public void SetPeriodShouldRequireEndDateGreaterThanStartDate()
    {
        var campaign = new Campaign();
        var startDateUtc = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);

        Should.Throw<ArgumentException>(() => campaign.SetPeriod(startDateUtc, startDateUtc));
        Should.Throw<ArgumentException>(() => campaign.SetPeriod(startDateUtc, startDateUtc.AddDays(-1)));
    }
}
