using MediatR;
using TaskManager.Domain;

namespace TaskManager.Application.Queries;

public record GetTaskByIdQuery(Guid id) : IRequest<TaskItem?>;
