using FluentAssertions;
using TaskManager.Application.Handlers;
using TaskManager.Application.Queries;

namespace TaskManager.Tests.Queries;

public class SearchTasksQueryHandlerTests : TaskQueryTestBase
{
    private readonly SearchTasksQueryHandler _handler;

    public SearchTasksQueryHandlerTests()
    {
        _handler = new SearchTasksQueryHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasks_WhenNoSearchApplied()
    {
        var query = new SearchTasksQuery("");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(TestTasks);
    }

    [Fact]
    public async Task Handle_ShouldSearchByTitle()
    {
        var query = new SearchTasksQuery("Buy");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Buy groceries");
    }

    [Fact]
    public async Task Handle_ShouldSearchByDescription()
    {
        var query = new SearchTasksQuery("report");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Write report");
    }

    [Fact]
    public async Task Handle_ShouldSearchByTitleAndDescription()
    {
        var query = new SearchTasksQuery("grocer");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Title == "Buy groceries");
        result.Should().Contain(t => t.Title == "Grocery shopping");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoMatchingTasks()
    {
        var query = new SearchTasksQuery("NonexistentTask");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCancel()
    {
        var query = new SearchTasksQuery("");
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(query, tokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
