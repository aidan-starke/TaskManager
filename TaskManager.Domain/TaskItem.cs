namespace TaskManager.Domain;

public class TaskItem
{
    public Guid Id { get; init; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;

    public TaskItem(string title)
    {
        Id = Guid.NewGuid();
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }
}
