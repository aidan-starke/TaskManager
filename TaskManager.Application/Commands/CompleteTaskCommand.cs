using FluentResults;
using MediatR;

namespace TaskManager.Application.Commands;

public record CompleteTaskCommand(Guid Id, bool IsCompleted = true) : IRequest<Result>;
