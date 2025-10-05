using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class CreateTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<CreateTaskCommand, Guid>
{
    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem task = new(request.Title)
        {
            Description = request.Description,
            Priority = request.Priority,
            Tags = request.Tags ?? [],
            DueDate = request.DueDate,
        };

        await TaskRepository.AddAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
