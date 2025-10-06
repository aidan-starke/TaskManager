using FluentAssertions;
using Moq;
using TaskManager.Application.Handlers;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Tests.Queries;

public class SearchTasksQueryHandlerTests : TaskCommandTestBase
{
    private readonly SearchTasksQueryHandler _handler;
    private readonly List<TaskItem> _testTasks;

    public SearchTasksQueryHandlerTests()
    {
        _handler = new SearchTasksQueryHandler(MockRepo.Object);

        _testTasks =
        [
            new TaskItem(
                "Buy groceries",
                "Get milk and eggs",
                ["shopping", "urgent"],
                DateTime.Now.AddDays(1),
                TaskPriority.High
            ),
            new TaskItem(
                "Write report",
                "Complete quarterly report",
                ["work", "documents"],
                DateTime.Now.AddDays(7),
                TaskPriority.Medium
            ),
            new TaskItem(
                "Call dentist",
                "Schedule appointment",
                ["health", "urgent"],
                DateTime.Now.AddDays(-1),
                TaskPriority.Low
            ),
            new TaskItem(
                "Grocery shopping",
                "Weekly shopping",
                ["shopping"],
                null,
                TaskPriority.Medium
            ),
        ];

        _testTasks[2].IsCompleted = true;

        MockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_testTasks);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasks_WhenNoSearchApplied()
    {
        var query = new SearchTasksQuery("");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(_testTasks);
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
