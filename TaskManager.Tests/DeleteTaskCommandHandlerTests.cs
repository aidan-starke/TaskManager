using FluentAssertions;
using Moq;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers;

namespace TaskManager.Tests;

public class DeleteTaskCommandHandlerTests : TaskCommandTestBase
{
    private readonly DeleteTaskCommandHandler _handler;

    public DeleteTaskCommandHandlerTests()
    {
        _handler = new DeleteTaskCommandHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTask()
    {
        var command = new DeleteTaskCommand(TestTaskId);

        await _handler.Handle(command, CancellationToken.None);

        MockRepo.Verify(
            r => r.DeleteAsync(It.Is<Guid>(t => t == TestTaskId), It.IsAny<CancellationToken>()),
            Times.Once
        );
        MockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelTask()
    {
        var command = new DeleteTaskCommand(TestTaskId);

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
