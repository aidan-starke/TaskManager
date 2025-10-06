using FluentAssertions;
using Moq;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Tests.Commands;

public class CompleteTaskCommandHandlerTests : TaskCommandTestBase
{
    private readonly CompleteTaskCommandHandler _handler;

    public CompleteTaskCommandHandlerTests()
    {
        _handler = new CompleteTaskCommandHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompleteTask()
    {
        var command = new CompleteTaskCommand(TestTaskId, true);

        await _handler.Handle(command, CancellationToken.None);

        MockRepo.Verify(
            r =>
                r.UpdateAsync(
                    It.Is<TaskItem>(t => t.IsCompleted == true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        MockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        var command = new CompleteTaskCommand(Guid.NewGuid());
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCancel()
    {
        var command = new CompleteTaskCommand(TestTaskId);

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
