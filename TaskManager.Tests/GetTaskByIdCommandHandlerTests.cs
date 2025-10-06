using FluentAssertions;
using Moq;
using TaskManager.Application.Handlers;
using TaskManager.Application.Queries;

namespace TaskManager.Tests;

public class GetTaskByIdQueryHandlerTests : TaskCommandTestBase
{
    private readonly GetTaskByIdQueryHandler _handler;

    public GetTaskByIdQueryHandlerTests()
    {
        _handler = new GetTaskByIdQueryHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTask()
    {
        var command = new GetTaskByIdQuery(TestTaskId);

        var result = await _handler.Handle(command, CancellationToken.None);

        MockRepo.Verify(
            r => r.GetByIdAsync(It.Is<Guid>(id => id == TestTaskId), It.IsAny<CancellationToken>()),
            Times.Once
        );

        result.Should().BeEquivalentTo(TestTask);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTaskNotFound()
    {
        var invalidId = Guid.NewGuid();
        var command = new GetTaskByIdQuery(invalidId);

        var result = await _handler.Handle(command, CancellationToken.None);

        MockRepo.Verify(
            r => r.GetByIdAsync(It.Is<Guid>(id => id == invalidId), It.IsAny<CancellationToken>()),
            Times.Once
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCancel()
    {
        var command = new GetTaskByIdQuery(TestTaskId);

        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(command, tokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
