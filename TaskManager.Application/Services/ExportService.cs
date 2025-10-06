using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Services;

public class ExportService(string FileName)
{
    public async Task ExportTasksAysnc(
        IEnumerable<TaskItem> tasks,
        IExportStrategy strategy,
        CancellationToken cancellationToken
    )
    {
        var content = await strategy.ExportAsync(tasks, cancellationToken);
        File.WriteAllText(FileName + strategy.FileExtension, content);
    }
}
