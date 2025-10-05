namespace TaskManager.Domain;

public class TaskItem(
    string title,
    string? description,
    List<string>? tags,
    DateTime? dueDate,
    TaskPriority priority = TaskPriority.Low
)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = title;
    public string? Description { get; set; } = description;
    public TaskPriority Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags ?? new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; } = dueDate;
    public bool IsCompleted { get; set; } = false;
}
