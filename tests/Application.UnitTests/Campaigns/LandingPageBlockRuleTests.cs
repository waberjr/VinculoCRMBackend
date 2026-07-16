using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Commands.CreateLandingPageBlockRule;
using VinculoBackend.Application.Campaigns.Commands.DeactivateLandingPageBlockRule;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class LandingPageBlockRuleTests
{
    [Test]
    public async Task CreateShouldStoreManualBlockWithExpiration()
    {
        var organizationId = Guid.NewGuid();
        var rules = new List<LandingPageBlockRule>();
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(item => item.LandingPageBlockRules).Returns(TestDbSetFactory.Create(rules));
        context.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var organization = new Mock<IOrganizationContext>();
        organization.SetupGet(item => item.HasOrganization).Returns(true);
        organization.SetupGet(item => item.OrganizationId).Returns(organizationId);
        var user = new Mock<IUser>();
        user.SetupGet(item => item.Id).Returns("admin");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        var handler = new CreateLandingPageBlockRuleCommandHandler(context.Object, organization.Object, user.Object, TimeProvider.System);
        var id = await handler.Handle(new CreateLandingPageBlockRuleCommand("campaign", Guid.NewGuid(), "fingerprint", null, "teste", expiresAt), CancellationToken.None);

        rules.Single().Id.ShouldBe(id);
        rules.Single().OrganizationId.ShouldBe(organizationId);
        rules.Single().FingerprintHash.ShouldBe("fingerprint");
        rules.Single().ExpiresAtUtc.ShouldBe(expiresAt);
        rules.Single().IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task DeactivateShouldTurnRuleInactive()
    {
        var rule = new LandingPageBlockRule
        {
            OrganizationId = Guid.NewGuid(),
            TargetType = "campaign",
            TargetId = Guid.NewGuid(),
            Source = "qr",
            IsActive = true,
        };
        var rules = new List<LandingPageBlockRule> { rule };
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(item => item.LandingPageBlockRules).Returns(TestDbSetFactory.Create(rules));
        context.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeactivateLandingPageBlockRuleCommandHandler(context.Object);
        await handler.Handle(new DeactivateLandingPageBlockRuleCommand(rule.Id), CancellationToken.None);

        rule.IsActive.ShouldBeFalse();
    }
}
