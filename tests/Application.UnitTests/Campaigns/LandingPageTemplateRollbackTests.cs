using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Commands.RollbackLandingPageTemplate;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class LandingPageTemplateRollbackTests
{
    [Test]
    public async Task HandleShouldRestoreSnapshotAndCreateNewVersion()
    {
        var organizationId = Guid.NewGuid();
        var template = LandingPageTemplate.Create(organizationId, "Atual", "Titulo atual", "Atual", null, 20, "[]");
        template.Update("Atual", "Titulo v2", "Atualizado", null, 30, "[]", true);
        var originalVersion = new LandingPageTemplateVersion
        {
            OrganizationId = organizationId,
            TemplateId = template.Id,
            Version = 1,
            Name = "Original",
            Title = "Titulo original",
            Subtitle = "Texto original",
            GoalAmount = 10,
            CustomFieldsJson = "[]",
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
        };
        var templates = new List<LandingPageTemplate> { template };
        var versions = new List<LandingPageTemplateVersion> { originalVersion };
        var audits = new List<LandingPageAuditEntry>();
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(item => item.LandingPageTemplates).Returns(TestDbSetFactory.Create(templates));
        context.SetupGet(item => item.LandingPageTemplateVersions).Returns(TestDbSetFactory.Create(versions));
        context.SetupGet(item => item.LandingPageAuditEntries).Returns(TestDbSetFactory.Create(audits));
        context.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var user = new Mock<IUser>();
        user.SetupGet(item => item.Id).Returns("admin");

        var handler = new RollbackLandingPageTemplateCommandHandler(context.Object, user.Object, TimeProvider.System);
        await handler.Handle(new RollbackLandingPageTemplateCommand(template.Id, 1), CancellationToken.None);

        template.Name.ShouldBe("Original");
        template.Title.ShouldBe("Titulo original");
        template.Version.ShouldBe(3);
        versions.Count.ShouldBe(2);
        versions.Last().Version.ShouldBe(3);
        audits.Single().Action.ShouldBe("RolledBack");
    }
}
