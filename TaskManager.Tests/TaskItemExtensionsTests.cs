using FluentAssertions;
using TaskManager.Application.Extensions;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class TaskItemExtensionsTests
{
    private readonly List<TaskItem> _testTasks;

    public TaskItemExtensionsTests()
    {
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
    }

    [Fact]
    public void WhereTitle_ShouldFilterByTitle()
    {
        var filteredTasks = _testTasks.WhereTitle("grocer");
        filteredTasks.Should().HaveCount(2);
        filteredTasks.First().Title.Should().Be("Buy groceries");
        filteredTasks.Last().Title.Should().Be("Grocery shopping");
    }

    [Fact]
    public void WhereDescription_ShouldFilterByDescription()
    {
        var filteredTasks = _testTasks.WhereDescription("Get milk and eggs");
        filteredTasks.Should().HaveCount(1);
        filteredTasks.First().Description.Should().Be("Get milk and eggs");
    }

    [Fact]
    public void WhereDescription_ShouldReturnAllTasks_WhenDescriptionIsNull()
    {
        var filteredTasks = _testTasks.WhereDescription(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void WherePriority_ShouldFilterByPriority()
    {
        var filteredTasks = _testTasks.WherePriority(TaskPriority.Medium);
        filteredTasks.Should().HaveCount(2);
        filteredTasks.Should().AllSatisfy(t => t.Priority.Should().Be(TaskPriority.Medium));
    }

    [Fact]
    public void WherePriority_ShouldReturnAllTasks_WhenPriorityIsNull()
    {
        var filteredTasks = _testTasks.WherePriority(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void WhereTags_ShouldFilterByTags()
    {
        var filteredTasks = _testTasks.WhereTags(["urgent"]);
        filteredTasks.Should().HaveCount(2);
        filteredTasks.Should().Contain(t => t.Title == "Buy groceries");
        filteredTasks.Should().Contain(t => t.Title == "Call dentist");
    }

    [Fact]
    public void WhereTags_ShouldFilterByMultipleTags()
    {
        var filteredTasks = _testTasks.WhereTags(["shopping", "work"]);
        filteredTasks.Should().HaveCount(3);
    }

    [Fact]
    public void WhereTags_ShouldReturnAllTasks_WhenTagsIsNull()
    {
        var filteredTasks = _testTasks.WhereTags(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void WhereCompleted_ShouldFilterCompletedTasks()
    {
        var filteredTasks = _testTasks.WhereCompleted(true);
        filteredTasks.Should().HaveCount(1);
        filteredTasks.First().Title.Should().Be("Call dentist");
    }

    [Fact]
    public void WhereCompleted_ShouldFilterIncompleteTasks()
    {
        var filteredTasks = _testTasks.WhereCompleted(false);
        filteredTasks.Should().HaveCount(3);
    }

    [Fact]
    public void WhereCompleted_ShouldReturnAllTasks_WhenIsCompletedIsNull()
    {
        var filteredTasks = _testTasks.WhereCompleted(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void WhereDueBefore_ShouldFilterTasksDueBeforeDate()
    {
        var tomorrow = DateTime.Now.AddDays(2);
        var filteredTasks = _testTasks.WhereDueBefore(tomorrow);
        filteredTasks.Should().HaveCount(2);
        filteredTasks.Should().Contain(t => t.Title == "Buy groceries");
        filteredTasks.Should().Contain(t => t.Title == "Call dentist");
    }

    [Fact]
    public void WhereDueBefore_ShouldReturnAllTasks_WhenDueBeforeIsNull()
    {
        var filteredTasks = _testTasks.WhereDueBefore(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void WhereDueAfter_ShouldFilterTasksDueAfterDate()
    {
        var tomorrow = DateTime.Now.AddDays(2);
        var filteredTasks = _testTasks.WhereDueAfter(tomorrow);
        filteredTasks.Should().HaveCount(1);
        filteredTasks.First().Title.Should().Be("Write report");
    }

    [Fact]
    public void WhereDueAfter_ShouldReturnAllTasks_WhenDueAfterIsNull()
    {
        var filteredTasks = _testTasks.WhereDueAfter(null);
        filteredTasks.Should().HaveCount(4);
    }

    [Fact]
    public void ChainedFilters_ShouldApplyMultipleFilters()
    {
        var filteredTasks = _testTasks
            .WhereTags(["shopping"])
            .WherePriority(TaskPriority.High)
            .WhereCompleted(false);

        filteredTasks.Should().HaveCount(1);
        filteredTasks.First().Title.Should().Be("Buy groceries");
    }

    [Fact]
    public void ChainedFilters_ShouldWorkWithDateFilters()
    {
        var now = DateTime.Now;
        var filteredTasks = _testTasks
            .WhereDueBefore(now.AddDays(10))
            .WhereCompleted(false)
            .WherePriority(TaskPriority.Medium);

        filteredTasks.Should().HaveCount(1);
        filteredTasks.First().Title.Should().Be("Write report");
    }

    [Fact]
    public void ChainedFilters_ShouldHandleNullParameters()
    {
        var filteredTasks = _testTasks
            .WhereTitle(null)
            .WhereDescription(null)
            .WherePriority(TaskPriority.Medium)
            .WhereTags(null);

        filteredTasks.Should().HaveCount(2);
        filteredTasks.Should().AllSatisfy(t => t.Priority.Should().Be(TaskPriority.Medium));
    }
}
