using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class RelationshipTaskTests
{
    [Test]
    public void StartShouldRequireOpenTask()
    {
        var task = new RelationshipTask { Status = RelationshipTaskStatus.Completed };

        Should.Throw<InvalidOperationDomainException>(() => task.Start());
    }

    [Test]
    public void CompleteShouldRequireOpenOrInProgressTask()
    {
        var task = new RelationshipTask { Status = RelationshipTaskStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() =>
            task.Complete(ContactOutcome.Reached, "ok", DateTimeOffset.UtcNow));
    }

    [Test]
    public void CancelShouldRequireOpenOrInProgressTask()
    {
        var task = new RelationshipTask { Status = RelationshipTaskStatus.Completed };

        Should.Throw<InvalidOperationDomainException>(() => task.Cancel());
    }
}
