using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Commands.ApplyLandingPageTemplateToType;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class ApplyLandingPageTemplateToTypeTests
{
    [Test]
    public async Task HandleShouldApplyOnlyPublishedPagesWhenFilterIsEnabled()
    {
        var organizationId = Guid.NewGuid();
        var template = LandingPageTemplate.Create(organizationId, "Base", "Titulo novo", "Sub", null, null, "[]", "Campanhas");
        var published = LandingPage.Create(organizationId, "campaign", Guid.NewGuid(), "Antigo", null, null, null, true, true, "[]", DateTimeOffset.UtcNow, null, 30, 7);
        var draft = LandingPage.Create(organizationId, "campaign", Guid.NewGuid(), "Rascunho", null, null, null, true, false);
        var project = LandingPage.Create(organizationId, "project", Guid.NewGuid(), "Projeto", null, null, null, true, true);
        var auditEntries = new List<LandingPageAuditEntry>();
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(item => item.LandingPageTemplates).Returns(TestDbSet([template]));
        context.SetupGet(item => item.LandingPages).Returns(TestDbSet([published, draft, project]));
        context.SetupGet(item => item.LandingPageAuditEntries).Returns(TestDbSet(auditEntries));
        context.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var organizationContext = new Mock<IOrganizationContext>();
        organizationContext.SetupGet(item => item.HasOrganization).Returns(true);
        organizationContext.SetupGet(item => item.OrganizationId).Returns(organizationId);
        var user = new Mock<IUser>();
        user.SetupGet(item => item.Id).Returns("admin");
        var handler = new ApplyLandingPageTemplateToTypeCommandHandler(context.Object, organizationContext.Object, user.Object, TimeProvider.System);

        var result = await handler.Handle(new ApplyLandingPageTemplateToTypeCommand(template.Id, "campaign", OnlyPublished: true), CancellationToken.None);

        result.UpdatedCount.ShouldBe(1);
        published.Title.ShouldBe("Titulo novo");
        published.SubmissionLimitWindowMinutes.ShouldBe(30);
        published.SubmissionLimitMaxAttempts.ShouldBe(7);
        draft.Title.ShouldBe("Rascunho");
        project.Title.ShouldBe("Projeto");
        auditEntries.Count.ShouldBe(1);
    }

    private static DbSet<T> TestDbSet<T>(IList<T> data)
        where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(item => item.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(item => item.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(item => item.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(item => item.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(item => item.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        mockSet.Setup(item => item.Add(It.IsAny<T>())).Callback<T>(data.Add);
        return mockSet.Object;
    }

    private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, [expression]);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [executionResult])!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());

        public T Current => _inner.Current;
    }
}
