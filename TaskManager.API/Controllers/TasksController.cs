using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands;
using TaskManager.Application.Queries;
using TaskManager.Domain;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks(
        CancellationToken cancellationToken
    )
    {
        var tasks = await mediator.Send(new GetAllTasksQuery(), cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItem>> GetTask(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var task = await mediator.Send(new GetTaskByIdQuery(id), cancellationToken);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTask(
        [FromBody] CreateTaskDto dto,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateTaskCommand(
            dto.Title,
            dto.Description,
            dto.Tags,
            dto.DueDate,
            dto.Priority
        );

        var taskId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetTask),
            new { id = taskId },
            new { id = taskId }
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskDto dto,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateTaskCommand(
            id,
            dto.Title,
            dto.Description,
            dto.Tags,
            dto.DueDate,
            dto.Priority
        );

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailed)
            return NotFound(result.Errors[0].Message);

        return NoContent();
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTask(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new CompleteTaskCommand(id), cancellationToken);
        if (result.IsFailed)
            return NotFound(result.Errors[0].Message);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTaskCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> FilterTasks(
        [FromQuery] string? title,
        [FromQuery] string? description,
        [FromQuery] bool? isCompleted,
        [FromQuery] TaskPriority? priority,
        [FromQuery] List<string>? tags,
        [FromQuery] DateTime? dueBefore,
        [FromQuery] DateTime? dueAfter,
        [FromQuery] TaskSortField? sortBy,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = new FilterTasksQuery(
            title,
            description,
            isCompleted,
            priority,
            tags,
            dueBefore,
            dueAfter,
            sortBy,
            sortDescending
        );

        var tasks = await mediator.Send(query, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> SearchTasks(
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken
    )
    {
        var tasks = await mediator.Send(new SearchTasksQuery(searchTerm), cancellationToken);
        return Ok(tasks);
    }
}

public record CreateTaskDto(
    string Title,
    string? Description,
    List<string>? Tags,
    DateTime? DueDate,
    TaskPriority Priority
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    List<string>? Tags,
    DateTime? DueDate,
    TaskPriority Priority
);
