using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class DonationPlanTests
{
    [Test]
    public void SetExpectedAmountShouldRequirePositiveValue()
    {
        var plan = new DonationPlan();

        Should.Throw<DomainValidationException>(() => plan.SetExpectedAmount(0));
        Should.Throw<DomainValidationException>(() => plan.SetExpectedAmount(-1));
    }

    [Test]
    public void SetBillingDayShouldAcceptDaysFromOneToThirtyOne()
    {
        var plan = new DonationPlan();

        plan.SetBillingDay(1);
        plan.BillingDay.ShouldBe(1);

        plan.SetBillingDay(31);
        plan.BillingDay.ShouldBe(31);
    }

    [Test]
    public void SetBillingDayShouldRejectDaysOutsideRecurringBillingRange()
    {
        var plan = new DonationPlan();

        Should.Throw<DomainValidationException>(() => plan.SetBillingDay(0));
        Should.Throw<DomainValidationException>(() => plan.SetBillingDay(32));
    }

    [Test]
    public void PauseShouldRequireActivePlan()
    {
        var plan = new DonationPlan { Status = DonationPlanStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() => plan.Pause(DateTimeOffset.UtcNow));
    }

    [Test]
    public void ResumeShouldRequirePausedPlan()
    {
        var plan = new DonationPlan { Status = DonationPlanStatus.Active };

        Should.Throw<InvalidOperationDomainException>(() => plan.Resume());
    }
}
