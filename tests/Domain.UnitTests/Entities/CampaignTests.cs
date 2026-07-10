using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class CampaignTests
{
    [Test]
    public void CreateShouldNormalizeCampaignData()
    {
        var startDateUtc = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(30);

        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "  Campanha Julho  ",
            "  Mobilizacao mensal  ",
            CampaignType.Fundraising,
            CampaignChannel.Email,
            15000,
            startDateUtc,
            endDateUtc,
            "user-1");

        campaign.Name.ShouldBe("Campanha Julho");
        campaign.Description.ShouldBe("Mobilizacao mensal");
        campaign.Type.ShouldBe(CampaignType.Fundraising);
        campaign.Channel.ShouldBe(CampaignChannel.Email);
        campaign.GoalAmount.ShouldBe(15000);
        campaign.Status.ShouldBe(CampaignStatus.Draft);
        campaign.AssignedUserId.ShouldBe("user-1");
        campaign.StartDateUtc.ShouldBe(startDateUtc);
        campaign.EndDateUtc.ShouldBe(endDateUtc);
    }

    [Test]
    public void CreateShouldRejectBlankName()
    {
        Should.Throw<DomainValidationException>(() => Campaign.Create(
            Guid.NewGuid(),
            " ",
            null,
            CampaignType.Fundraising,
            null,
            100,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            null));
    }

    [Test]
    public void SetGoalAmountShouldRequirePositiveValue()
    {
        var campaign = new Campaign();

        Should.Throw<DomainValidationException>(() => campaign.SetGoalAmount(null));
        Should.Throw<DomainValidationException>(() => campaign.SetGoalAmount(0));
        Should.Throw<DomainValidationException>(() => campaign.SetGoalAmount(-1));
    }

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
