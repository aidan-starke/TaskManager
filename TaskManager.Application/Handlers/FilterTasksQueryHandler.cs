using MediatR;
using TaskManager.Application.Extensions;
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

        return (await TaskRepository.GetAllAsync(cancellationToken))
            .WhereTitle(request.Title)
            .WhereDescription(request.Description)
            .WhereCompleted(request.IsCompleted)
            .WherePriority(request.Priority)
            .WhereTags(request.Tags)
            .WhereDueBefore(request.DueBefore)
            .WhereDueAfter(request.DueAfter)
            .OrderBy(request.SortBy, request.SortDescending);
    }
}
