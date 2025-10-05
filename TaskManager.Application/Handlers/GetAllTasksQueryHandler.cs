using MediatR;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class GetAllTasksQueryHandler(ITaskRepository TaskRepository)
    : IRequestHandler<GetAllTasksQuery, IEnumerable<TaskItem>>
{
    public async Task<IEnumerable<TaskItem>> Handle(
        GetAllTasksQuery request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await TaskRepository.GetAllAsync(cancellationToken);
    }
}
