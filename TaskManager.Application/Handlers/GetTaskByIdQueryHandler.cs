using MediatR;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class GetTaskByIdQueryHandler(ITaskRepository TaskRepository)
    : IRequestHandler<GetTaskByIdQuery, TaskItem?>
{
    public async Task<TaskItem?> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await TaskRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
