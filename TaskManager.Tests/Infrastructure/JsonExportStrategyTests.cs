using System.Text.Json;
using FluentAssertions;
using TaskManager.Domain;
using TaskManager.Infrastructure.Export;

namespace TaskManager.Tests.Infrastructure;

public class JsonExportStrategyTests
{
    private readonly JsonExportStrategy _strategy = new();

    [Fact]
    public void FileExtension_ShouldReturnJson()
    {
        _strategy.FileExtension.Should().Be(".json");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyList_ShouldReturnEmptyJsonArray()
    {
        var tasks = new List<TaskItem>();

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);

        result.Should().Be("[]");
    }

    [Fact]
    public async Task ExportAsync_WithSingleTask_ShouldSerializeToJson()
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
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
        result.Should().Contain("\"Priority\":2");
    }

    [Fact]
    public async Task ExportAsync_ShouldProduceValidJson()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);

        var act = () => JsonSerializer.Deserialize<List<TaskItem>>(result);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExportAsync_ShouldBeDeserializable()
    {
        var task = new TaskItem(
            "Test Task",
            "Test Description",
            new List<string> { "tag1", "tag2" },
            new DateTime(2025, 12, 31),
            TaskPriority.High
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized.Should().NotBeNull();
        deserialized.Should().HaveCount(1);
        deserialized![0].Title.Should().Be("Test Task");
        deserialized[0].Description.Should().Be("Test Description");
        deserialized[0].Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2" });
        deserialized[0].Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task ExportAsync_WithNullDescription_ShouldSerializeCorrectly()
    {
        var task = new TaskItem("Task", null, null, null, TaskPriority.Medium);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].Description.Should().BeNull();
    }

    [Fact]
    public async Task ExportAsync_WithNullDueDate_ShouldSerializeCorrectly()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].DueDate.Should().BeNull();
    }

    [Fact]
    public async Task ExportAsync_WithCompletedTask_ShouldSerializeCompletedStatus()
    {
        var task = new TaskItem("Completed Task", null, null, null, TaskPriority.Low)
        {
            IsCompleted = true,
        };

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_WithMultipleTasks_ShouldSerializeAll()
    {
        var tasks = new[]
        {
            new TaskItem("Task 1", "Description 1", null, null, TaskPriority.Low),
            new TaskItem("Task 2", "Description 2", null, null, TaskPriority.High),
            new TaskItem("Task 3", "Description 3", null, null, TaskPriority.Medium),
        };

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized.Should().HaveCount(3);
        deserialized![0].Title.Should().Be("Task 1");
        deserialized[1].Title.Should().Be("Task 2");
        deserialized[2].Title.Should().Be("Task 3");
    }

    [Fact]
    public async Task ExportAsync_ShouldPreserveTaskId()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task ExportAsync_ShouldPreserveCreatedAt()
    {
        var task = new TaskItem("Task", "Description", null, null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].CreatedAt.Should().BeCloseTo(task.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExportAsync_WithEmptyTags_ShouldSerializeEmptyArray()
    {
        var task = new TaskItem("Task", "Description", new List<string>(), null, TaskPriority.Low);

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportAsync_WithSpecialCharacters_ShouldEscapeCorrectly()
    {
        var task = new TaskItem(
            "Task with \"quotes\" and \n newlines",
            "Description with special chars: \\ / \" '",
            null,
            null,
            TaskPriority.Low
        );

        var result = await _strategy.ExportAsync(new[] { task }, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].Title.Should().Be("Task with \"quotes\" and \n newlines");
        deserialized[0].Description.Should().Be("Description with special chars: \\ / \" '");
    }

    [Fact]
    public async Task ExportAsync_WithAllPriorities_ShouldSerializeCorrectly()
    {
        var tasks = new[]
        {
            new TaskItem("Low", null, null, null, TaskPriority.Low),
            new TaskItem("Medium", null, null, null, TaskPriority.Medium),
            new TaskItem("High", null, null, null, TaskPriority.High),
        };

        var result = await _strategy.ExportAsync(tasks, CancellationToken.None);
        var deserialized = JsonSerializer.Deserialize<List<TaskItem>>(result);

        deserialized![0].Priority.Should().Be(TaskPriority.Low);
        deserialized[1].Priority.Should().Be(TaskPriority.Medium);
        deserialized[2].Priority.Should().Be(TaskPriority.High);
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
