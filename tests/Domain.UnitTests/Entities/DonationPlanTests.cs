using VinculoBackend.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class DonationPlanTests
{
    [Test]
    public void SetExpectedAmountShouldRequirePositiveValue()
    {
        var plan = new DonationPlan();

        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetExpectedAmount(0));
        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetExpectedAmount(-1));
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

        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetBillingDay(0));
        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetBillingDay(32));
    }
}
