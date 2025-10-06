using MediatR;
using TaskManager.Application.Extensions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class SearchTasksQueryHandler(ITaskRepository TaskRepository)
    : IRequestHandler<SearchTasksQuery, IEnumerable<TaskItem>>
{
    public async Task<IEnumerable<TaskItem>> Handle(
        SearchTasksQuery request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tasks = await TaskRepository.GetAllAsync(cancellationToken);

        var titleMatches = tasks.WhereTitle(request.SearchTerm);
        var descriptionMatches = tasks.WhereDescription(request.SearchTerm);

        return titleMatches.Union(descriptionMatches);
    }
}
