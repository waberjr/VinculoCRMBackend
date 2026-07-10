using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class DonationTests
{
    [Test]
    public void CreateShouldNormalizeDonationData()
    {
        var expectedAtUtc = new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero);

        var donation = Donation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            100,
            DonationType.OneTime,
            DonationStatus.Pending,
            PaymentMethod.Pix,
            expectedAtUtc,
            null,
            "  REF-001  ",
            "  EXT-001  ",
            "  Observacao  ",
            "user-1");

        donation.Amount.ShouldBe(100);
        donation.Status.ShouldBe(DonationStatus.Pending);
        donation.ExpectedAtUtc.ShouldBe(expectedAtUtc);
        donation.Reference.ShouldBe("REF-001");
        donation.ExternalPaymentId.ShouldBe("EXT-001");
        donation.Notes.ShouldBe("Observacao");
        donation.CreatedByUserId.ShouldBe("user-1");
    }

    [Test]
    public void CreateShouldRequirePaidDateForConfirmedDonation()
    {
        Should.Throw<DomainValidationException>(() => Donation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            100,
            DonationType.OneTime,
            DonationStatus.Confirmed,
            PaymentMethod.Pix,
            null,
            null,
            null,
            null,
            null,
            null));
    }

    [Test]
    public void CreateShouldRequireExpectedDateForPendingDonation()
    {
        Should.Throw<DomainValidationException>(() => Donation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            100,
            DonationType.OneTime,
            DonationStatus.Pending,
            PaymentMethod.Pix,
            null,
            null,
            null,
            null,
            null,
            null));
    }

    [Test]
    public void CancelAndRefundShouldRequireReason()
    {
        var pending = new Donation { Status = DonationStatus.Pending };
        Should.Throw<DomainValidationException>(() => pending.Cancel(" ", DateTimeOffset.UtcNow));

        var confirmed = new Donation { Status = DonationStatus.Confirmed };
        Should.Throw<DomainValidationException>(() => confirmed.Refund(" ", DateTimeOffset.UtcNow));
    }
}
