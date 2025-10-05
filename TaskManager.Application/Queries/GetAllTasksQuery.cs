using MediatR;
using TaskManager.Domain;

namespace TaskManager.Application.Queries;

public record GetAllTasksQuery() : IRequest<IEnumerable<TaskItem>>;
