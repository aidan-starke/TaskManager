using MediatR;

namespace TaskManager.Application.Commands;

public record DeleteTaskCommand(Guid id) : IRequest<Unit>;
