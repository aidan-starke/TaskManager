using FluentAssertions;
using TaskManager.Application.Handlers;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Tests.Queries;

public class FilterTasksQueryHandlerTests : TaskQueryTestBase
{
    private readonly FilterTasksQueryHandler _handler;

    public FilterTasksQueryHandlerTests()
    {
        _handler = new FilterTasksQueryHandler(MockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasks_WhenNoFiltersApplied()
    {
        var query = new FilterTasksQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(TestTasks);
    }

    [Fact]
    public async Task Handle_ShouldFilterByTitle()
    {
        var query = new FilterTasksQuery(Title: "Buy");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Buy groceries");
    }

    [Fact]
    public async Task Handle_ShouldFilterByTitle_PartialMatch()
    {
        var query = new FilterTasksQuery(Title: "Grocery");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Grocery shopping");
    }

    [Fact]
    public async Task Handle_ShouldFilterByDescription()
    {
        var query = new FilterTasksQuery(Description: "report");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Write report");
    }

    [Fact]
    public async Task Handle_ShouldFilterByIsCompleted_True()
    {
        var query = new FilterTasksQuery(IsCompleted: true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Call dentist");
        result.First().IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByIsCompleted_False()
    {
        var query = new FilterTasksQuery(IsCompleted: false);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.All(t => !t.IsCompleted).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByPriority()
    {
        var query = new FilterTasksQuery(Priority: TaskPriority.High);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Buy groceries");
        result.First().Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task Handle_ShouldFilterByTags_SingleTag()
    {
        var query = new FilterTasksQuery(Tags: ["urgent"]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Title == "Buy groceries");
        result.Should().Contain(t => t.Title == "Call dentist");
    }

    [Fact]
    public async Task Handle_ShouldFilterByTags_MultipleTags()
    {
        var query = new FilterTasksQuery(Tags: ["shopping", "work"]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Title == "Buy groceries");
        result.Should().Contain(t => t.Title == "Grocery shopping");
        result.Should().Contain(t => t.Title == "Write report");
    }

    [Fact]
    public async Task Handle_ShouldFilterByDueBefore()
    {
        var query = new FilterTasksQuery(DueBefore: DateTime.Now.AddDays(2));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Title == "Buy groceries");
        result.Should().Contain(t => t.Title == "Call dentist");
    }

    [Fact]
    public async Task Handle_ShouldFilterByDueAfter()
    {
        var query = new FilterTasksQuery(DueAfter: DateTime.Now.AddDays(5));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Write report");
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateRange()
    {
        var query = new FilterTasksQuery(
            DueAfter: DateTime.Now.AddDays(-2),
            DueBefore: DateTime.Now.AddDays(2)
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldCombineMultipleFilters()
    {
        var query = new FilterTasksQuery(
            Tags: ["shopping"],
            IsCompleted: false,
            Priority: TaskPriority.High
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Buy groceries");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoMatchingTasks()
    {
        var query = new FilterTasksQuery(Title: "NonexistentTask");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCancel()
    {
        var query = new FilterTasksQuery();
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var act = async () => await _handler.Handle(query, tokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
