using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Export;

public class MarkdownExportStrategy : IExportStrategy
{
    public string FileExtension => ".md";

    public Task<string> ExportAsync(
        IEnumerable<TaskItem> tasks,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var markdown = GenerateMarkdown(tasks);

        return Task.FromResult(markdown);
    }

    private static string GenerateMarkdown(IEnumerable<TaskItem> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
        {
            return "# Tasks\n\nNo tasks available.\n";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Tasks");
        sb.AppendLine();
        sb.AppendLine("| Status | Title | Priority | Due Date | Tags | Description |");
        sb.AppendLine("|--------|-------|----------|----------|------|-------------|");

        foreach (var task in taskList)
        {
            var status = task.IsCompleted ? "✓" : " ";
            var title = EscapeMarkdown(task.Title);
            var priority = task.Priority.ToString();
            var dueDate = task.DueDate?.ToString("yyyy-MM-dd") ?? "-";
            var tags = task.Tags.Count > 0 ? string.Join(", ", task.Tags) : "-";
            var description = task.Description != null ? EscapeMarkdown(task.Description) : "-";

            sb.AppendLine(
                $"| {status} | {title} | {priority} | {dueDate} | {tags} | {description} |"
            );
        }

        return sb.ToString();
    }

    private static string EscapeMarkdown(string text)
    {
        return text.Replace("|", "\\|").Replace("\n", " ").Replace("\r", "");
    }
}
