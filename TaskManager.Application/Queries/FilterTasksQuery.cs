using MediatR;
using TaskManager.Domain;

namespace TaskManager.Application.Queries;

public record FilterTasksQuery(
    string? Title = null,
    string? Description = null,
    bool? IsCompleted = null,
    TaskPriority? Priority = null,
    List<string>? Tags = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null,
    TaskSortField? SortBy = null,
    bool SortDescending = false
) : IRequest<IEnumerable<TaskItem>>;
