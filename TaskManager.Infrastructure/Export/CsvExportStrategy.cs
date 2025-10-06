using System.Text;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Export;

public class CsvExportStrategy : IExportStrategy
{
    public string FileExtension => ".csv";

    public Task<string> ExportAsync(
        IEnumerable<TaskItem> tasks,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        StringBuilder csv = new();

        csv.AppendLine("Id,CreatedAt,Title,Description,Tags,DueDate,Priority,IsCompleted");

        foreach (var task in tasks)
        {
            csv.AppendLine(
                $"{task.Id},"
                    + $"{task.CreatedAt:yyyy-MM-dd},"
                    + $"\"{EscapeCsv(task.Title)}\","
                    + $"\"{EscapeCsv(task.Description ?? string.Empty)}\","
                    + $"\"{string.Join(";", task.Tags)}\","
                    + $"{task.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty},"
                    + $"{task.Priority},"
                    + $"{task.IsCompleted}"
            );
        }

        return Task.FromResult(csv.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"'))
            return value.Replace("\"", "\"\"");

        return value;
    }
}
