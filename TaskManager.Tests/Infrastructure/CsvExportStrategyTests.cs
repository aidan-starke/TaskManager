using FluentAssertions;
using TaskManager.Domain;
using TaskManager.Infrastructure.Export;

namespace TaskManager.Tests.Infrastructure;

public class CsvExportStrategyTests
{
    private readonly CsvExportStrategy _strategy = new();

    [Fact]
    public void FileExtension_ShouldReturnCsv()
    {
        _strategy.FileExtension.Should().Be(".csv");
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeHeaderRow()
    {
        var tasks = new List<TaskItem>();

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        result.Should().Contain("Id,CreatedAt,Title,Description,Tags,DueDate,Priority,IsCompleted");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyList_ShouldReturnOnlyHeader()
    {
        var tasks = new List<TaskItem>();

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1);
        lines[0].Should().Be("Id,CreatedAt,Title,Description,Tags,DueDate,Priority,IsCompleted");
    }

    [Fact]
    public async Task ExportAsync_WithSingleTask_ShouldExportAllFields()
    {
        var task = new TaskItem(
            "Test Task",
            "Test Description",
            new List<string> { "tag1", "tag2" },
            new DateTime(2025, 12, 31),
            TaskPriority.High
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain(task.Id.ToString());
        result.Should().Contain("Test Task");
        result.Should().Contain("Test Description");
        result.Should().Contain("tag1;tag2");
        result.Should().Contain("2025-12-31");
        result.Should().Contain("High");
        result.Should().Contain("False");
    }

    [Fact]
    public async Task ExportAsync_WithCompletedTask_ShouldShowCompletedStatus()
    {
        var task = new TaskItem("Completed Task", null, null, null, TaskPriority.Low)
        {
            IsCompleted = true,
        };

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("True");
    }

    [Fact]
    public async Task ExportAsync_WithNullDescription_ShouldExportEmptyString()
    {
        var task = new TaskItem("Task", null, null, null, TaskPriority.Medium);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[1].Should().Contain("\"\"");
    }

    [Fact]
    public async Task ExportAsync_WithNullDueDate_ShouldExportEmptyString()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var fields = lines[1].Split(',');
        fields[5].Should().BeEmpty(); // DueDate field
    }

    [Fact]
    public async Task ExportAsync_WithEmptyTags_ShouldExportEmptyQuotedString()
    {
        var task = new TaskItem("Task", "Description", new List<string>(), null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("\"\"");
    }

    [Fact]
    public async Task ExportAsync_WithMultipleTags_ShouldJoinWithSemicolon()
    {
        var task = new TaskItem(
            "Task",
            "Description",
            new List<string> { "urgent", "work", "project" },
            null,
            TaskPriority.High
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("\"urgent;work;project\"");
    }

    [Fact]
    public async Task ExportAsync_WithQuotesInTitle_ShouldEscapeQuotes()
    {
        var task = new TaskItem(
            "Task with \"quotes\"",
            "Description",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("\"Task with \"\"quotes\"\"\"");
    }

    [Fact]
    public async Task ExportAsync_WithQuotesInDescription_ShouldEscapeQuotes()
    {
        var task = new TaskItem(
            "Task",
            "Description with \"quotes\"",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("\"Description with \"\"quotes\"\"\"");
    }

    [Fact]
    public async Task ExportAsync_WithMultipleTasks_ShouldExportAllTasks()
    {
        var tasks = new[]
        {
            new TaskItem("Task 1", "Description 1", null, null, TaskPriority.Low),
            new TaskItem("Task 2", "Description 2", null, null, TaskPriority.High),
            new TaskItem("Task 3", "Description 3", null, null, TaskPriority.Medium),
        };

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(4); // 1 header + 3 tasks
        result.Should().Contain("Task 1");
        result.Should().Contain("Task 2");
        result.Should().Contain("Task 3");
    }

    [Fact]
    public async Task ExportAsync_ShouldFormatCreatedAtAsYYYYMMDD()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var fields = lines[1].Split(',');
        fields[1].Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}$");
    }

    [Fact]
    public async Task ExportAsync_ShouldFormatDueDateAsYYYYMMDD()
    {
        var task = new TaskItem(
            "Task",
            "Description",
            null,
            new DateTime(2025, 10, 15),
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("2025-10-15");
    }

    [Fact]
    public async Task ExportAsync_WithAllPriorities_ShouldExportCorrectly()
    {
        var tasks = new[]
        {
            new TaskItem("Low", null, null, null, TaskPriority.Low),
            new TaskItem("Medium", null, null, null, TaskPriority.Medium),
            new TaskItem("High", null, null, null, TaskPriority.High),
        };

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        result.Should().Contain("Low");
        result.Should().Contain("Medium");
        result.Should().Contain("High");
    }

    [Fact]
    public async Task ExportAsync_WithCancellationRequested_ShouldThrowOperationCanceledException()
    {
        var tasks = new[] { new TaskItem("Task", null, null, null, TaskPriority.Low) };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _strategy.ExportAsync(tasks, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
