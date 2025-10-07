using FluentAssertions;
using Moq;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class UpdateTaskCommandHandlerTests : TaskCommandTestBase
{
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        _handler = new UpdateTaskCommandHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTask()
    {
        var command = new UpdateTaskCommand(TestTaskId, "Test Task Updated", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        MockRepo.Verify(
            r =>
                r.UpdateAsync(
                    It.Is<TaskItem>(t => t.Title == "Test Task Updated"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        MockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancel()
    {
        var command = new UpdateTaskCommand(TestTaskId, "Test Task Updated", null, null, null);

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var taskId = Guid.NewGuid();
        var command = new UpdateTaskCommand(taskId, "Non-existent Task", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Message.Should()
            .Be($"Task with id {taskId} not found");
    }
}
