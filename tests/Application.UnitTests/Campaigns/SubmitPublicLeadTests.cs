using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Commands.SubmitPublicLead;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class SubmitPublicLeadTests
{
    [Test]
    public async Task ExpiredManualBlockRuleShouldNotRejectLead()
    {
        var organizationId = Guid.NewGuid();
        var campaign = Campaign.Create(
            organizationId,
            "Campanha",
            null,
            CampaignType.Fundraising,
            null,
            1000,
            null,
            null,
            null);
        var rules = new List<LandingPageBlockRule>
        {
            new()
            {
                OrganizationId = organizationId,
                TargetType = "campaign",
                TargetId = campaign.Id,
                Source = "qr",
                IsActive = true,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(-1),
            },
        };
        var attempts = new List<LandingPageSubmissionAttempt>();
        var donors = new List<Donor>();
        var context = Context([campaign], [], rules, attempts, donors);
        var handler = new SubmitPublicLeadCommandHandler(context.Object, TimeProvider.System);

        var result = await handler.Handle(new SubmitPublicLeadCommand
        {
            TargetType = "campaign",
            TargetId = campaign.Id,
            FullName = "Maria Silva",
            Email = "maria@example.org",
            Source = "qr",
            IpAddress = "127.0.0.1",
            UserAgent = "unit-test",
        }, CancellationToken.None);

        result.Created.ShouldBeTrue();
        donors.Count.ShouldBe(1);
        attempts.Count.ShouldBe(1);
        attempts.Single().Blocked.ShouldBeFalse();
    }

    [Test]
    public async Task ActiveManualBlockRuleShouldRejectLeadAndRegisterBlockedAttempt()
    {
        var organizationId = Guid.NewGuid();
        var campaign = Campaign.Create(
            organizationId,
            "Campanha",
            null,
            CampaignType.Fundraising,
            null,
            1000,
            null,
            null,
            null);
        var rules = new List<LandingPageBlockRule>
        {
            new()
            {
                OrganizationId = organizationId,
                TargetType = "campaign",
                TargetId = campaign.Id,
                Source = "qr",
                IsActive = true,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
            },
        };
        var attempts = new List<LandingPageSubmissionAttempt>();
        var donors = new List<Donor>();
        var context = Context([campaign], [], rules, attempts, donors);
        var handler = new SubmitPublicLeadCommandHandler(context.Object, TimeProvider.System);

        await Should.ThrowAsync<VinculoBackend.Application.Common.Exceptions.ValidationException>(() => handler.Handle(new SubmitPublicLeadCommand
        {
            TargetType = "campaign",
            TargetId = campaign.Id,
            FullName = "Maria Silva",
            Email = "maria@example.org",
            Source = "qr",
            IpAddress = "127.0.0.1",
            UserAgent = "unit-test",
        }, CancellationToken.None));

        donors.ShouldBeEmpty();
        attempts.Count.ShouldBe(1);
        attempts.Single().Blocked.ShouldBeTrue();
        attempts.Single().Reason.ShouldBe("manual-block-rule");
    }

    private static Mock<IApplicationDbContext> Context(
        IList<Campaign> campaigns,
        IList<LandingPage> pages,
        IList<LandingPageBlockRule> rules,
        IList<LandingPageSubmissionAttempt> attempts,
        IList<Donor> donors)
    {
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(item => item.Campaigns).Returns(TestDbSetFactory.Create(campaigns));
        context.SetupGet(item => item.Projects).Returns(TestDbSetFactory.Create(new List<Project>()));
        context.SetupGet(item => item.LandingPages).Returns(TestDbSetFactory.Create(pages));
        context.SetupGet(item => item.LandingPageBlockRules).Returns(TestDbSetFactory.Create(rules));
        context.SetupGet(item => item.LandingPageSubmissionAttempts).Returns(TestDbSetFactory.Create(attempts));
        context.SetupGet(item => item.LandingPageAuditEntries).Returns(TestDbSetFactory.Create(new List<LandingPageAuditEntry>()));
        context.SetupGet(item => item.OperationalAlerts).Returns(TestDbSetFactory.Create(new List<OperationalAlert>()));
        context.SetupGet(item => item.Donors).Returns(TestDbSetFactory.Create(donors));
        context.SetupGet(item => item.Donations).Returns(TestDbSetFactory.Create(new List<Donation>()));
        context.SetupGet(item => item.DonationProjects).Returns(TestDbSetFactory.Create(new List<DonationProject>()));
        context.SetupGet(item => item.DonorTimelineEntries).Returns(TestDbSetFactory.Create(new List<DonorTimelineEntry>()));
        context.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return context;
    }
}
