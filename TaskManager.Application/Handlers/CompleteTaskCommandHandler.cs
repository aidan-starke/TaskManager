using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class CompleteTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<CompleteTaskCommand, Unit>
{
    public async Task<Unit> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? task = await TaskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task == null)
            throw new KeyNotFoundException($"Task with id {request.Id} not found");

        task.IsCompleted = request.IsCompleted;

        await TaskRepository.UpdateAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
