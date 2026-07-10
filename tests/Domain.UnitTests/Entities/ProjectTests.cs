using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class ProjectTests
{
    [Test]
    public void CreateShouldNormalizeProjectData()
    {
        var startDateUtc = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(30);

        var project = Project.Create(
            Guid.NewGuid(),
            "  Projeto Escola  ",
            "  Apoio educacional  ",
            5000,
            "  100 alunos atendidos  ",
            ProjectStatus.Active,
            startDateUtc,
            endDateUtc);

        project.Name.ShouldBe("Projeto Escola");
        project.Description.ShouldBe("Apoio educacional");
        project.ImpactMetric.ShouldBe("100 alunos atendidos");
        project.GoalAmount.ShouldBe(5000);
        project.Status.ShouldBe(ProjectStatus.Active);
        project.StartDateUtc.ShouldBe(startDateUtc);
        project.EndDateUtc.ShouldBe(endDateUtc);
    }

    [Test]
    public void CreateShouldRejectBlankName()
    {
        Should.Throw<DomainValidationException>(() => Project.Create(
            Guid.NewGuid(),
            " ",
            null,
            null,
            null,
            ProjectStatus.Draft,
            null,
            null));
    }

    [Test]
    public void SetGoalAmountShouldRequirePositiveValue()
    {
        var project = new Project();

        Should.Throw<DomainValidationException>(() => project.SetGoalAmount(0));
        Should.Throw<DomainValidationException>(() => project.SetGoalAmount(-1));
    }

    [Test]
    public void SetPeriodShouldRequireEndDateGreaterThanStartDate()
    {
        var project = new Project();
        var startDateUtc = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);

        Should.Throw<DomainValidationException>(() => project.SetPeriod(startDateUtc, startDateUtc));
        Should.Throw<DomainValidationException>(() => project.SetPeriod(startDateUtc, startDateUtc.AddDays(-1)));
    }
}
