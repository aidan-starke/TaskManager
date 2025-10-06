using MediatR;

namespace TaskManager.Application.Commands;

public record DeleteTaskCommand(Guid Id) : IRequest<Unit>;
