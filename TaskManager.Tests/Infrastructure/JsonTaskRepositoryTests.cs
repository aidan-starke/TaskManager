using FluentAssertions;
using TaskManager.Domain;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Tests;

public class JsonTaskRepositoryTests : IDisposable
{
    private readonly string _testFilePath;

    public JsonTaskRepositoryTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), "test_tasks.json");
    }

    public void Dispose()
    {
        File.Delete(_testFilePath);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTask_ToInMemoryList()
    {
        var repo = new JsonTaskRepository(_testFilePath);
        var task = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);

        await repo.AddAsync(task);

        var result = await repo.GetByIdAsync(task.Id, CancellationToken.None);
        result?.Should().Be(task);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistTasks_ToFile()
    {
        var repo = new JsonTaskRepository(_testFilePath);
        var task = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);

        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        var newRepo = new JsonTaskRepository(_testFilePath);
        var tasks = await newRepo.GetAllAsync();
        tasks
            .Should()
            .ContainSingle(t =>
                t.Title == "Test Task"
                && t.Description == "Test Description"
                && t.Tags.SequenceEqual(new List<string>())
                && t.DueDate == null
                && t.Priority == TaskPriority.Low
            );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTask_InMemoryList()
    {
        var repo = new JsonTaskRepository(_testFilePath);
        var task = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);

        await repo.AddAsync(task);
        var updatedTask = await repo.GetByIdAsync(task.Id, CancellationToken.None);
        if (updatedTask == null)
            throw new KeyNotFoundException($"Task with id {task.Id} not found");

        updatedTask.IsCompleted = true;
        await repo.UpdateAsync(updatedTask);

        var updatedTasks = await repo.GetAllAsync();
        updatedTasks.Should().Contain(updatedTask);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        var repo = new JsonTaskRepository(_testFilePath);
        var task = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);

        await repo.AddAsync(task);
        var tasks = await repo.GetAllAsync();
        var taskItem = tasks.First();
        await repo.DeleteAsync(taskItem.Id);

        var yeet = await repo.GetByIdAsync(taskItem.Id, CancellationToken.None);
        yeet.Should().BeNull();
    }
}
