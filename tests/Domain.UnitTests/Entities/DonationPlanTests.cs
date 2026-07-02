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
    public void SetBillingDayShouldAcceptDaysFromOneToTwentyEight()
    {
        var plan = new DonationPlan();

        plan.SetBillingDay(1);
        plan.BillingDay.ShouldBe(1);

        plan.SetBillingDay(28);
        plan.BillingDay.ShouldBe(28);
    }

    [Test]
    public void SetBillingDayShouldRejectDaysOutsideRecurringBillingRange()
    {
        var plan = new DonationPlan();

        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetBillingDay(0));
        Should.Throw<ArgumentOutOfRangeException>(() => plan.SetBillingDay(29));
    }
}
