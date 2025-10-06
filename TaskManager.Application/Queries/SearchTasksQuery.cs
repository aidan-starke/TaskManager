using MediatR;
using TaskManager.Domain;

namespace TaskManager.Application.Queries;

public record SearchTasksQuery(string SearchTerm) : IRequest<IEnumerable<TaskItem>>;
