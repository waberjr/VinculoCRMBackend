using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Domain.UnitTests.Entities;

public class RelationshipTaskTests
{
    [Test]
    public void CreateShouldNormalizeTaskData()
    {
        var dueAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero);

        var task = RelationshipTask.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "  Ligar para doador  ",
            "  Confirmar interesse  ",
            "agent-1",
            "admin-1",
            TaskType.Call,
            TaskPriority.High,
            dueAtUtc);

        task.Title.ShouldBe("Ligar para doador");
        task.Description.ShouldBe("Confirmar interesse");
        task.AssignedUserId.ShouldBe("agent-1");
        task.CreatedByUserId.ShouldBe("admin-1");
        task.Type.ShouldBe(TaskType.Call);
        task.Priority.ShouldBe(TaskPriority.High);
        task.Status.ShouldBe(RelationshipTaskStatus.Open);
        task.DueAtUtc.ShouldBe(dueAtUtc);
    }

    [Test]
    public void CreateShouldRejectBlankTitle()
    {
        Should.Throw<DomainValidationException>(() => RelationshipTask.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            " ",
            null,
            null,
            null,
            TaskType.Call,
            TaskPriority.Medium,
            null));
    }

    [Test]
    public void UpdateShouldRejectCompletedOrCancelledTask()
    {
        var completed = new RelationshipTask { Status = RelationshipTaskStatus.Completed };
        var cancelled = new RelationshipTask { Status = RelationshipTaskStatus.Cancelled };

        Should.Throw<InvalidOperationDomainException>(() => Update(completed));
        Should.Throw<InvalidOperationDomainException>(() => Update(cancelled));
    }

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

    private static void Update(RelationshipTask task)
    {
        task.Update(
            Guid.NewGuid(),
            null,
            null,
            "Contato",
            null,
            null,
            TaskType.Call,
            TaskPriority.Medium,
            null);
    }
}
