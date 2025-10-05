using FluentAssertions;
using Moq;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateTask_AndReturnId()
    {
        var mockRepo = new Mock<ITaskRepository>();
        var handler = new CreateTaskCommandHandler(mockRepo.Object);
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            null,
            null,
            TaskPriority.Low
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        mockRepo.Verify(
            r =>
                r.AddAsync(
                    It.Is<TaskItem>(t =>
                        t.Title == "Test Task"
                        && t.Description == "Test Description"
                        && t.Tags.SequenceEqual(new List<string>())
                        && t.DueDate == null
                        && t.Priority == TaskPriority.Low
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelTask()
    {
        var mockRepo = new Mock<ITaskRepository>();
        var handler = new CreateTaskCommandHandler(mockRepo.Object);
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            null,
            null,
            TaskPriority.Low
        );

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
