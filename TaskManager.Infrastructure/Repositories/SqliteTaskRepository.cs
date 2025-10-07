using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class SqliteTaskRepository(TaskDbContext context) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Tasks.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context.Tasks.ToListAsync(cancellationToken);
    }

    public async Task<Guid> AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await context.Tasks.AddAsync(task, cancellationToken);
        return task.Id;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Update(task);
        return;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetByIdAsync(id, cancellationToken);
        if (task != null)
        {
            context.Tasks.Remove(task);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
