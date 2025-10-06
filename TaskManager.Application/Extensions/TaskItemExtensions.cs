using TaskManager.Domain;

namespace TaskManager.Application.Extensions;

public static class TaskItemExtensions
{
    public static IEnumerable<TaskItem> WhereTitle(
        this IEnumerable<TaskItem> tasks,
        string? title
    ) =>
        title is not null
            ? tasks.Where(t => t.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            : tasks;

    public static IEnumerable<TaskItem> WhereDescription(
        this IEnumerable<TaskItem> tasks,
        string? description
    ) =>
        description is not null
            ? tasks.Where(t =>
                t.Description?.Contains(description, StringComparison.OrdinalIgnoreCase) ?? false
            )
            : tasks;

    public static IEnumerable<TaskItem> WherePriority(
        this IEnumerable<TaskItem> tasks,
        TaskPriority? priority
    ) => priority is not null ? tasks.Where(t => t.Priority == priority) : tasks;

    public static IEnumerable<TaskItem> WhereTags(
        this IEnumerable<TaskItem> tasks,
        IEnumerable<string>? tags
    ) => tags is not null ? tasks.Where(t => t.Tags.Intersect(tags).Any()) : tasks;

    public static IEnumerable<TaskItem> WhereCompleted(
        this IEnumerable<TaskItem> tasks,
        bool? isCompleted
    ) => isCompleted is not null ? tasks.Where(t => t.IsCompleted == isCompleted) : tasks;

    public static IEnumerable<TaskItem> WhereDueBefore(
        this IEnumerable<TaskItem> tasks,
        DateTime? dueBefore
    ) => dueBefore is not null ? tasks.Where(t => t.DueDate <= dueBefore) : tasks;

    public static IEnumerable<TaskItem> WhereDueAfter(
        this IEnumerable<TaskItem> tasks,
        DateTime? dueAfter
    ) => dueAfter is not null ? tasks.Where(t => t.DueDate >= dueAfter) : tasks;
}
