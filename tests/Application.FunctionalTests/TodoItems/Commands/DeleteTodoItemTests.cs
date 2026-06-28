using VinculoBackend.Application.TodoItems.Commands.CreateTodoItem;
using VinculoBackend.Application.TodoItems.Commands.DeleteTodoItem;
using VinculoBackend.Application.TodoLists.Commands.CreateTodoList;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.FunctionalTests.TodoItems.Commands;

public class DeleteTodoItemTests : TestBase
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new DeleteTodoItemCommand(Guid.NewGuid());

        await Should.ThrowAsync<VinculoBackend.Application.Common.Exceptions.NotFoundException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldDeleteTodoItem()
    {
        var listId = await TestApp.SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await TestApp.SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "New Item"
        });

        await TestApp.SendAsync(new DeleteTodoItemCommand(itemId));

        var item = await TestApp.FindAsync<TodoItem>(itemId);

        item.ShouldBeNull();
    }
}
