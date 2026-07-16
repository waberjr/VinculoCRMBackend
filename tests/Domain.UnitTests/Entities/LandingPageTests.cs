using NUnit.Framework;
using Shouldly;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class LandingPageTests
{
    [Test]
    public void LandingPageShouldStoreSubmissionLimits()
    {
        var page = LandingPage.Create(
            Guid.NewGuid(),
            "campaign",
            Guid.NewGuid(),
            "Titulo",
            null,
            null,
            null,
            true,
            true,
            "[]",
            DateTimeOffset.UtcNow,
            null,
            submissionLimitWindowMinutes: 30,
            submissionLimitMaxAttempts: 7);

        page.SubmissionLimitWindowMinutes.ShouldBe(30);
        page.SubmissionLimitMaxAttempts.ShouldBe(7);
    }

    [Test]
    public void LandingPageShouldRejectInvalidSubmissionLimits()
    {
        Should.Throw<DomainValidationException>(() => LandingPage.Create(
            Guid.NewGuid(),
            "campaign",
            Guid.NewGuid(),
            "Titulo",
            null,
            null,
            null,
            true,
            true,
            submissionLimitWindowMinutes: 0,
            submissionLimitMaxAttempts: 7));

        Should.Throw<DomainValidationException>(() => LandingPage.Create(
            Guid.NewGuid(),
            "campaign",
            Guid.NewGuid(),
            "Titulo",
            null,
            null,
            null,
            true,
            true,
            submissionLimitWindowMinutes: 30,
            submissionLimitMaxAttempts: 0));
    }

    [Test]
    public void LandingPageTemplateShouldIncrementVersionOnContentUpdate()
    {
        var template = LandingPageTemplate.Create(Guid.NewGuid(), "Base", "Titulo", null, null, null, "[]");

        template.Version.ShouldBe(1);

        template.Update("Base", "Titulo novo", null, null, null, "[]", true);

        template.Version.ShouldBe(2);
    }
}
