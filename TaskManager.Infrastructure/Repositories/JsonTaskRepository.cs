using System.Text.Json;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Repositories;

public class JsonTaskRepository : ITaskRepository
{
    private readonly Lazy<List<TaskItem>> TaskItems = new();
    private FileInfo _FileInfo { get; set; }

    public JsonTaskRepository(string filePath)
    {
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");

        _FileInfo = new(filePath);

        TaskItems = new Lazy<List<TaskItem>>(() =>
        {
            if (_FileInfo.Length == 0)
                return new();

            return JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(filePath)) ?? new();
        });
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return TaskItems.Value.FirstOrDefault(x => x.Id == id);
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return TaskItems.Value;
    }

    public async Task<Guid> AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItems.Value.Add(task);

        return task.Id;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? existingTask = TaskItems.Value.FirstOrDefault(x => x.Id == task.Id);
        if (existingTask != null)
        {
            TaskItems.Value.Remove(existingTask);
            TaskItems.Value.Add(task);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? task = TaskItems.Value.FirstOrDefault(x => x.Id == id);
        if (task != null)
            TaskItems.Value.Remove(task);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var fileStream = _FileInfo.Open(FileMode.Create);
        await JsonSerializer.SerializeAsync(
            fileStream,
            TaskItems.Value,
            cancellationToken: cancellationToken
        );
    }
}
