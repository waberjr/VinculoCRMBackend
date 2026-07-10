using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
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

        Should.Throw<DomainValidationException>(() => campaign.SetPeriod(startDateUtc, startDateUtc));
        Should.Throw<DomainValidationException>(() => campaign.SetPeriod(startDateUtc, startDateUtc.AddDays(-1)));
    }

    [Test]
    public void CompleteShouldRejectCancelledCampaign()
    {
        var campaign = new Campaign { Status = CampaignStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() => campaign.Complete());
    }

    [Test]
    public void ActivateShouldRejectFinishedCampaigns()
    {
        Should.Throw<InvalidOperationDomainException>(() => new Campaign { Status = CampaignStatus.Completed }.Activate());
        Should.Throw<InvalidOperationDomainException>(() => new Campaign { Status = CampaignStatus.Cancelled }.Activate());
    }
}
