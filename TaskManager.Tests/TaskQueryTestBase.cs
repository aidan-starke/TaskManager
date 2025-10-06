using Moq;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class TaskQueryTestBase : TaskCommandTestBase
{
    protected readonly List<TaskItem> TestTasks;

    public TaskQueryTestBase()
    {
        TestTasks = new()
        {
            new TaskItem(
                "Buy groceries",
                "Get milk and eggs",
                ["shopping", "urgent"],
                DateTime.Now.AddDays(1),
                TaskPriority.High
            ),
            new TaskItem(
                "Write report",
                "Complete quarterly report",
                ["work", "documents"],
                DateTime.Now.AddDays(7),
                TaskPriority.Medium
            ),
            new TaskItem(
                "Call dentist",
                "Schedule appointment",
                ["health", "urgent"],
                DateTime.Now.AddDays(-1),
                TaskPriority.Low
            ),
            new TaskItem(
                "Grocery shopping",
                "Weekly shopping",
                ["shopping"],
                null,
                TaskPriority.Medium
            ),
        };

        TestTasks[2].IsCompleted = true;

        MockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestTasks);
    }
}
