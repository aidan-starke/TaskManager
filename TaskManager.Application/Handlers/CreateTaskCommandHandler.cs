using MediatR;
using TaskManager.Domain;

public class CreateTaskCommandHandler(ITaskRepository taskRepository) : IRequestHandler<CreateTaskCommand, Guid>
{
    public ITaskRepository TaskRepository { get; } = taskRepository;

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskItem task = new(request.Title)
        {
            Description = request.Description,
            Priority = request.Priority,
            Tags = request.Tags ?? [],
            DueDate = request.DueDate
        };

        await TaskRepository.AddAsync(task, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);


        return task.Id;
    }
}
