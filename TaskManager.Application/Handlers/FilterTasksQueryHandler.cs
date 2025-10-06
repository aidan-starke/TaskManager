using MediatR;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class FilterTasksQueryHandler(ITaskRepository TaskRepository)
    : IRequestHandler<FilterTasksQuery, IEnumerable<TaskItem>>
{
    public async Task<IEnumerable<TaskItem>> Handle(
        FilterTasksQuery request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<TaskItem> query = await TaskRepository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrEmpty(request.Title))
            query = query.Where(t => t.Title.Contains(request.Title));

        if (!string.IsNullOrEmpty(request.Description))
            query = query.Where(t => t.Description?.Contains(request.Description) ?? false);

        if (request.IsCompleted != null)
            query = query.Where(t => t.IsCompleted == request.IsCompleted);

        if (request.Priority != null)
            query = query.Where(t => t.Priority == request.Priority);

        if (request.Tags != null)
            query = query.Where(t => t.Tags.Intersect(request.Tags).Any());

        if (request.DueBefore != null)
            query = query.Where(t => t.DueDate <= request.DueBefore);

        if (request.DueAfter != null)
            query = query.Where(t => t.DueDate >= request.DueAfter);

        return query;
    }
}
