using Moq;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Tests;

public abstract class TaskCommandTestBase
{
    protected readonly Mock<ITaskRepository> MockRepo;
    protected readonly Guid TestTaskId;
    protected readonly TaskItem TestTask;

    protected TaskCommandTestBase()
    {
        MockRepo = new Mock<ITaskRepository>();
        TestTask = new TaskItem("Test Task", "Test Description", null, null, TaskPriority.Low);
        TestTaskId = TestTask.Id;

        MockRepo
            .Setup(r => r.GetByIdAsync(TestTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestTask);
    }
}
