using System.Text.Json;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Export;

public class JsonExportStrategy : IExportStrategy
{
    public string FileExtension => ".json";

    public Task<string> ExportAsync(
        IEnumerable<TaskItem> tasks,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var json = JsonSerializer.Serialize(tasks);

        return Task.FromResult(json);
    }
}
