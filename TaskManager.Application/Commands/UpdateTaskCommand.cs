using MediatR;
using TaskManager.Domain;

namespace TaskManager.Application.Commands;

public record UpdateTaskCommand(
    Guid id,
    string? Title,
    string? Description,
    List<String>? Tags,
    DateTime? DueDate,
    TaskPriority Priority = TaskPriority.Low
) : IRequest<Unit>;
