using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class LandingPageAuditTests
{
    [Test]
    public void CreateShouldNormalizeDescriptionAndKeepContext()
    {
        var organizationId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(2026, 7, 15, 10, 30, 0, TimeSpan.Zero);

        var entry = LandingPageAudit.Create(
            organizationId,
            "campaign",
            entityId,
            "Published",
            "Landing publicada",
            "  Campanha de inverno  ",
            occurredAt);

        entry.OrganizationId.ShouldBe(organizationId);
        entry.EntityType.ShouldBe("campaign");
        entry.EntityId.ShouldBe(entityId);
        entry.Action.ShouldBe("Published");
        entry.Description.ShouldBe("Campanha de inverno");
        entry.OccurredAtUtc.ShouldBe(occurredAt);
    }
}
