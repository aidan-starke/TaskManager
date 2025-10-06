using FluentAssertions;
using TaskManager.Domain;
using TaskManager.Infrastructure.Export;

namespace TaskManager.Tests.Infrastructure;

public class MarkdownExportStrategyTests
{
    private readonly MarkdownExportStrategy _strategy = new();

    [Fact]
    public void FileExtension_ShouldReturnMarkdown()
    {
        _strategy.FileExtension.Should().Be(".md");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyList_ShouldReturnNoTasksMessage()
    {
        var tasks = new List<TaskItem>();

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        result.Should().Contain("# Tasks");
        result.Should().Contain("No tasks available.");
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeHeaderAndTableHeader()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("# Tasks");
        result.Should().Contain("| Status | Title | Priority | Due Date | Tags | Description |");
        result.Should().Contain("|--------|-------|----------|----------|------|-------------|");
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

        result.Should().Contain("Test Task");
        result.Should().Contain("Test Description");
        result.Should().Contain("tag1, tag2");
        result.Should().Contain("2025-12-31");
        result.Should().Contain("High");
    }

    [Fact]
    public async Task ExportAsync_WithUncompletedTask_ShouldShowEmptyCheckbox()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("|   |");
    }

    [Fact]
    public async Task ExportAsync_WithCompletedTask_ShouldShowCheckmark()
    {
        var task = new TaskItem("Completed Task", null, null, null, TaskPriority.Low)
        {
            IsCompleted = true,
        };

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("| ✓ |");
    }

    [Fact]
    public async Task ExportAsync_WithNullDescription_ShouldShowDash()
    {
        var task = new TaskItem("Task", null, null, null, TaskPriority.Medium);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var lines = result.Split('\n');
        var dataLine = lines.First(l =>
            l.StartsWith("| ") && l.Contains("Task") && !l.Contains("---")
        );
        dataLine.Should().EndWith("| - |");
    }

    [Fact]
    public async Task ExportAsync_WithNullDueDate_ShouldShowDash()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("| - |");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyTags_ShouldShowDash()
    {
        var task = new TaskItem("Task", "Description", new List<string>(), null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("| - |");
    }

    [Fact]
    public async Task ExportAsync_WithMultipleTags_ShouldJoinWithComma()
    {
        var task = new TaskItem(
            "Task",
            "Description",
            new List<string> { "urgent", "work", "project" },
            null,
            TaskPriority.High
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("urgent, work, project");
    }

    [Fact]
    public async Task ExportAsync_WithPipeCharacterInTitle_ShouldEscape()
    {
        var task = new TaskItem("Task | with | pipes", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("Task \\| with \\| pipes");
    }

    [Fact]
    public async Task ExportAsync_WithPipeCharacterInDescription_ShouldEscape()
    {
        var task = new TaskItem("Task", "Description | with | pipes", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("Description \\| with \\| pipes");
    }

    [Fact]
    public async Task ExportAsync_WithNewlineInTitle_ShouldReplaceWithSpace()
    {
        var task = new TaskItem(
            "Task\nwith\nnewlines",
            "Description",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("Task with newlines");
        result.Should().NotContain("Task\n");
    }

    [Fact]
    public async Task ExportAsync_WithNewlineInDescription_ShouldReplaceWithSpace()
    {
        var task = new TaskItem(
            "Task",
            "Description\nwith\nnewlines",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().Contain("Description with newlines");
    }

    [Fact]
    public async Task ExportAsync_WithCarriageReturnInText_ShouldRemove()
    {
        var task = new TaskItem(
            "Task\r\nwith CRLF",
            "Description\r\n",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        result.Should().NotContain("\r");
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

        result.Should().Contain("Task 1");
        result.Should().Contain("Task 2");
        result.Should().Contain("Task 3");
        result.Should().Contain("Description 1");
        result.Should().Contain("Description 2");
        result.Should().Contain("Description 3");
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

        result.Should().Contain("| Low | Low |");
        result.Should().Contain("| Medium | Medium |");
        result.Should().Contain("| High | High |");
    }

    [Fact]
    public async Task ExportAsync_ShouldProduceValidMarkdownTable()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var tableHeaderLine = lines.First(l => l.StartsWith("| Status"));
        var separatorLine = lines.First(l => l.StartsWith("|---"));
        var dataLine = lines.First(l => l.Contains("Task") && l.StartsWith("|"));

        tableHeaderLine.Split('|').Should().HaveCount(8); // 6 columns + 2 edge pipes
        separatorLine.Split('|').Should().HaveCount(8);
        dataLine.Split('|').Should().HaveCount(8);
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
