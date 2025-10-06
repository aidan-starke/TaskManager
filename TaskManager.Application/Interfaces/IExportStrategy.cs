using TaskManager.Domain;

namespace TaskManager.Application.Interfaces;

public interface IExportStrategy
{
    string FileExtension { get; }

    Task<string> ExportAsync(IEnumerable<TaskItem> tasks, CancellationToken cancellationToken);
}
