using MediatR;

namespace TaskManager.Application.Commands;

public record CompleteTaskCommand(Guid id, bool IsCompleted = true) : IRequest<Unit>;
