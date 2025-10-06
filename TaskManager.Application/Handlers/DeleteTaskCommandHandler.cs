using MediatR;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;

namespace TaskManager.Application.Handlers;

public class DeleteTaskCommandHandler(ITaskRepository TaskRepository)
    : IRequestHandler<DeleteTaskCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await TaskRepository.DeleteAsync(request.Id, cancellationToken);
        await TaskRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
