using FluentResults;
using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class UpdateTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<UpdateTaskCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? task = await TaskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
            return Result.Fail($"Task with id {request.Id} not found");

        task.Title = request.Title ?? task.Title;
        task.Description = request.Description ?? task.Description;
        task.Tags = request.Tags ?? task.Tags;
        task.DueDate = request.DueDate ?? task.DueDate;
        task.Priority = request.Priority;

        await TaskRepository.UpdateAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
