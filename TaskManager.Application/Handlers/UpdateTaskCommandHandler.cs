using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class UpdateTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<UpdateTaskCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? task = await TaskRepository.GetByIdAsync(request.id, cancellationToken);
        if (task == null)
            throw new KeyNotFoundException($"Task with id {request.id} not found");

        task.Title = request.Title ?? task.Title;
        task.Description = request.Description ?? task.Description;
        task.Tags = request.Tags ?? task.Tags;
        task.DueDate = request.DueDate ?? task.DueDate;
        task.Priority = request.Priority;

        await TaskRepository.UpdateAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
