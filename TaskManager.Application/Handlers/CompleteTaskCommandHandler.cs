using FluentResults;
using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Handlers;

public class CompleteTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<CompleteTaskCommand, Result>
{
    public async Task<Result> Handle(
        CompleteTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem? task = await TaskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
            return Result.Fail($"Task with id {request.Id} not found");

        task.IsCompleted = request.IsCompleted;

        await TaskRepository.UpdateAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
