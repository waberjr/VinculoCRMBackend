using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class DonorTagTests
{
    [Test]
    public void CreateShouldNormalizeTagData()
    {
        var organizationId = Guid.NewGuid();

        var tag = DonorTag.Create(organizationId, "  Recorrente  ", "  Doadores recorrentes  ");

        tag.OrganizationId.ShouldBe(organizationId);
        tag.Name.ShouldBe("Recorrente");
        tag.Description.ShouldBe("Doadores recorrentes");
        tag.IsActive.ShouldBeTrue();
    }

    [Test]
    public void UpdateShouldAllowDeactivation()
    {
        var tag = DonorTag.Create(Guid.NewGuid(), "Major");

        tag.Update("  Grande doador  ", "  Alto valor  ", false);

        tag.Name.ShouldBe("Grande doador");
        tag.Description.ShouldBe("Alto valor");
        tag.IsActive.ShouldBeFalse();
    }

    [Test]
    public void CreateShouldRejectBlankName()
    {
        Should.Throw<DomainValidationException>(() => DonorTag.Create(Guid.NewGuid(), " "));
    }
}
