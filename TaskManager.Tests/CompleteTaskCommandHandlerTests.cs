using FluentAssertions;
using Moq;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class CompleteTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly CompleteTaskCommandHandler _handler;
    private readonly Guid _testTaskId;
    private readonly TaskItem _testTask;

    public CompleteTaskCommandHandlerTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _handler = new CompleteTaskCommandHandler(_mockRepo.Object);
        _testTask = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);
        _testTaskId = _testTask.Id;

        _mockRepo
            .Setup(r => r.GetByIdAsync(_testTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testTask);
    }

    [Fact]
    public async Task Handle_ShouldCompleteTask()
    {
        var command = new CompleteTaskCommand(_testTaskId, true);

        await _handler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(
            r =>
                r.UpdateAsync(
                    It.Is<TaskItem>(t => t.IsCompleted == true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        var command = new CompleteTaskCommand(Guid.NewGuid());
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCancelTask()
    {
        var command = new CompleteTaskCommand(_testTaskId);

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
