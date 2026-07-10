using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class ReceiptTests
{
    [Test]
    public void IssueShouldRequireConfirmedDonation()
    {
        var donation = new Donation { Status = DonationStatus.Pending };

        Should.Throw<InvalidOperationDomainException>(() =>
            Receipt.Issue(Guid.NewGuid(), donation, "REC-00001", "user", DateTimeOffset.UtcNow));
    }

    [Test]
    public void CancelShouldRejectAlreadyCancelledReceipt()
    {
        var receipt = new Receipt { Status = ReceiptStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() => receipt.Cancel("duplicado"));
    }

    [Test]
    public void ReissueShouldRejectCancelledReceipt()
    {
        var receipt = new Receipt { Status = ReceiptStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() => receipt.Reissue("user", DateTimeOffset.UtcNow));
    }
}
