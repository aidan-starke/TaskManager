using FluentAssertions;
using Moq;
using TaskManager.Application.Handlers;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class GetAllTasksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllTasks()
    {
        var mockRepo = new Mock<ITaskRepository>();
        List<TaskItem> expectedTasks =
        [
            new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low),
            new TaskItem("Test Task 2", "Test Description 2", null, null, TaskPriority.Medium),
        ];
        mockRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTasks);

        var handler = new GetAllTasksQueryHandler(mockRepo.Object);
        var command = new GetAllTasksQuery();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(expectedTasks);
    }

    [Fact]
    public async Task Handle_ShouldReturnNoTasksWhenEmpty()
    {
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var handler = new GetAllTasksQueryHandler(mockRepo.Object);
        var command = new GetAllTasksQuery();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(new List<TaskItem>());
    }

    [Fact]
    public async Task Handle_ShouldCancelTask()
    {
        var mockRepo = new Mock<ITaskRepository>();
        var handler = new GetAllTasksQueryHandler(mockRepo.Object);
        var command = new GetAllTasksQuery();

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
