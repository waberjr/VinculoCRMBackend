using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class CommunicationTests
{
    [Test]
    public void CampaignCreateShouldNormalizePlanAndSetStatus()
    {
        var scheduledAtUtc = DateTimeOffset.UtcNow.AddDays(1);

        var campaign = CommunicationCampaign.Create(
            Guid.NewGuid(),
            "  Boas vindas  ",
            CommunicationChannel.Email,
            "  Novos doadores  ",
            null,
            scheduledAtUtc,
            "user-1");

        campaign.Name.ShouldBe("Boas vindas");
        campaign.Audience.ShouldBe("Novos doadores");
        campaign.Status.ShouldBe(CommunicationCampaignStatus.Scheduled);
        campaign.CreatedByUserId.ShouldBe("user-1");
    }

    [Test]
    public void CampaignShouldCountBlockedRecipientsSeparately()
    {
        var organizationId = Guid.NewGuid();
        var campaign = CommunicationCampaign.Create(
            organizationId,
            "Retencao",
            CommunicationChannel.Email,
            "Ativos",
            null,
            null,
            null);

        campaign.AddRecipient(organizationId, Guid.NewGuid(), true, Guid.NewGuid());
        campaign.AddRecipient(organizationId, Guid.NewGuid(), false, null);

        campaign.RecipientsCount.ShouldBe(1);
        campaign.BlockedByConsentCount.ShouldBe(1);
        campaign.Recipients.ShouldContain(recipient => recipient.Status == CommunicationRecipientStatus.Blocked);
    }

    [Test]
    public void CampaignShouldRejectBlankNameOrAudience()
    {
        Should.Throw<DomainValidationException>(() => CommunicationCampaign.Create(
            Guid.NewGuid(),
            " ",
            CommunicationChannel.Email,
            "Ativos",
            null,
            null,
            null));

        Should.Throw<DomainValidationException>(() => CommunicationCampaign.Create(
            Guid.NewGuid(),
            "Retencao",
            CommunicationChannel.Email,
            " ",
            null,
            null,
            null));
    }

    [Test]
    public void TemplateShouldRejectBlankNameOrBody()
    {
        Should.Throw<DomainValidationException>(() => CommunicationTemplate.Create(
            Guid.NewGuid(),
            " ",
            CommunicationChannel.Email,
            null,
            "Conteudo",
            []));

        Should.Throw<DomainValidationException>(() => CommunicationTemplate.Create(
            Guid.NewGuid(),
            "Template",
            CommunicationChannel.Email,
            null,
            " ",
            []));
    }
}
