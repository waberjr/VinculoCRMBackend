using NUnit.Framework;
using Shouldly;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class LandingPageTemplateTests
{
    [Test]
    public void CreateShouldNormalizeTemplateData()
    {
        var template = LandingPageTemplate.Create(
            Guid.NewGuid(),
            "  Modelo principal  ",
            "  Doe agora  ",
            "  Apoie este projeto  ",
            "  https://example.com/image.png  ",
            1000,
            null,
            "  Emergencia  ");

        template.Name.ShouldBe("Modelo principal");
        template.Category.ShouldBe("Emergencia");
        template.Title.ShouldBe("Doe agora");
        template.Subtitle.ShouldBe("Apoie este projeto");
        template.HeroImageUrl.ShouldBe("https://example.com/image.png");
        template.GoalAmount.ShouldBe(1000);
        template.IsActive.ShouldBeTrue();
    }

    [Test]
    public void UpdateShouldRejectInvalidGoal()
    {
        var template = LandingPageTemplate.Create(Guid.NewGuid(), "Modelo", "Titulo", null, null, null, null);

        Should.Throw<DomainValidationException>(() => template.Update("Modelo", "Titulo", null, null, 0, null, true));
    }

    [Test]
    public void DeactivateAndActivateShouldChangeStatus()
    {
        var template = LandingPageTemplate.Create(Guid.NewGuid(), "Modelo", "Titulo", null, null, null, null);

        template.Deactivate();
        template.IsActive.ShouldBeFalse();

        template.Activate();
        template.IsActive.ShouldBeTrue();
    }
}
