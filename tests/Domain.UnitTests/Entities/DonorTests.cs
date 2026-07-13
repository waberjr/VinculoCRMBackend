using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class DonorTests
{
    [Test]
    public void CreateShouldNormalizeDonorData()
    {
        var donor = Donor.Create(
            Guid.NewGuid(),
            "  Maria Silva  ",
            DonorPersonType.Individual,
            DonorStatus.Lead,
            null,
            null,
            null,
            "  12345678901  ",
            "  maria@example.com  ",
            "  62999990000  ",
            null,
            null,
            "  Goiania  ",
            "  GO  ",
            "  Rua 1  ",
            null,
            "  74000000  ",
            true,
            false,
            null,
            "agent-1",
            null,
            "  Primeira abordagem  ");

        donor.FullName.ShouldBe("Maria Silva");
        donor.Document.ShouldBe("12345678901");
        donor.Email.ShouldBe("maria@example.com");
        donor.Phone.ShouldBe("62999990000");
        donor.City.ShouldBe("Goiania");
        donor.State.ShouldBe("GO");
        donor.AddressLine1.ShouldBe("Rua 1");
        donor.PostalCode.ShouldBe("74000000");
        donor.AllowsCommunication.ShouldBeTrue();
        donor.DoNotContact.ShouldBeFalse();
        donor.DoNotContactReason.ShouldBeNull();
        donor.Status.ShouldBe(DonorStatus.Lead);
        donor.Notes.ShouldBe("Primeira abordagem");
    }

    [Test]
    public void CreateShouldRejectBlankName()
    {
        Should.Throw<DomainValidationException>(() => Donor.Create(
            Guid.NewGuid(),
            " ",
            DonorPersonType.Individual,
            DonorStatus.Lead,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            false,
            null,
            null,
            null,
            null));
    }

    [Test]
    public void BlockContactShouldRequireReasonAndDisableCommunication()
    {
        var donor = Donor.Create(
            Guid.NewGuid(),
            "Carlos",
            DonorPersonType.Individual,
            DonorStatus.Active,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            false,
            null,
            null,
            null,
            null);

        Should.Throw<DomainValidationException>(() => donor.BlockContact(" "));

        donor.BlockContact(" Pediu para nao receber contatos ");

        donor.Status.ShouldBe(DonorStatus.DoNotContact);
        donor.AllowsCommunication.ShouldBeFalse();
        donor.DoNotContact.ShouldBeTrue();
        donor.DoNotContactReason.ShouldBe("Pediu para nao receber contatos");
    }

    [Test]
    public void UpdateShouldRejectDoNotContactStatusWithoutBlockReason()
    {
        var donor = new Donor();

        Should.Throw<DomainValidationException>(() => donor.Update(
            "Ana",
            DonorPersonType.Individual,
            DonorStatus.DoNotContact,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            false,
            null,
            null,
            null,
            null));
    }

    [Test]
    public void ReplacePhonesShouldKeepOnlyOnePrimaryAndUpdateSummaryFields()
    {
        var organizationId = Guid.NewGuid();
        var donorId = Guid.NewGuid();
        var donor = new Donor { Id = donorId, OrganizationId = organizationId };

        donor.ReplacePhones(
        [
            DonorPhone.Create(organizationId, donorId, PhoneType.Mobile, "  62999990000  ", true),
            DonorPhone.Create(organizationId, donorId, PhoneType.WhatsApp, "  62988880000  ", true),
        ]);

        donor.Phones.Count(phone => phone.IsPrimary).ShouldBe(1);
        donor.Phones.First().IsPrimary.ShouldBeTrue();
        donor.Phone.ShouldBe("62999990000");
        donor.WhatsApp.ShouldBe("62988880000");
    }

    [Test]
    public void ReplaceEmailsShouldKeepOnlyOnePrimaryAndUpdateSummaryField()
    {
        var organizationId = Guid.NewGuid();
        var donorId = Guid.NewGuid();
        var donor = new Donor { Id = donorId, OrganizationId = organizationId };

        donor.ReplaceEmails(
        [
            DonorEmail.Create(organizationId, donorId, EmailType.Personal, " pessoal@example.com ", false),
            DonorEmail.Create(organizationId, donorId, EmailType.Work, " trabalho@example.com ", false),
        ]);

        donor.Emails.Count(email => email.IsPrimary).ShouldBe(1);
        donor.Emails.First().IsPrimary.ShouldBeTrue();
        donor.Email.ShouldBe("pessoal@example.com");
    }
}
