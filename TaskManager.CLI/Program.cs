using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Application.Services;
using TaskManager.Domain;
using TaskManager.Infrastructure.Export;
using TaskManager.Infrastructure.Repositories;

var services = new ServiceCollection();

services.AddLogging();

services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly((typeof(CreateTaskCommand).Assembly)));

services.AddSingleton<ITaskRepository>(new JsonTaskRepository("tasks.json"));

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices(
                [
                    "Create Task",
                    "List All Tasks",
                    "View Task by ID",
                    "Complete Task",
                    "Update Task",
                    "Delete Task",
                    "Search Tasks",
                    "Filter Tasks",
                    "Export Tasks",
                    "Exit",
                ]
            )
    );

    switch (choice)
    {
        case "Create Task":
            await CreateTask(mediator);
            break;
        case "List All Tasks":
            await ListAllTasks(mediator);
            break;
        case "View Task by ID":
            await ViewTaskById(mediator);
            break;
        case "Complete Task":
            await CompleteTask(mediator);
            break;
        case "Update Task":
            await UpdateTask(mediator);
            break;
        case "Delete Task":
            await DeleteTask(mediator);
            break;
        case "Search Tasks":
            await SearchTasks(mediator);
            break;
        case "Filter Tasks":
            await FilterTasks(mediator);
            break;
        case "Export Tasks":
            await ExportTasks(mediator);
            break;
        case "Exit":
            return;
    }
}

async Task CreateTask(IMediator mediator)
{
    var title = AnsiConsole.Prompt(new TextPrompt<string>("Task Title"));
    var description = AnsiConsole.Confirm("Add Description?")
        ? AnsiConsole.Ask<string>("Description:")
        : null;

    var priority = AnsiConsole.Prompt(
        new SelectionPrompt<TaskPriority>()
            .Title("Select Priority:")
            .AddChoices(Enum.GetValues<TaskPriority>())
    );

    var addTags = AnsiConsole.Confirm("Add tags?");
    List<string>? tags = null;
    if (addTags)
    {
        var tagInput = AnsiConsole.Ask<string>("Enter tags (comma-separated):");
        tags = tagInput.Split(',').Select(t => t.Trim()).ToList();
    }

    var dueDate = AnsiConsole.Confirm("Add Due Date?")
        ? AnsiConsole.Ask<DateTime?>("Due Date:")
        : null;

    var taskId = await mediator.Send(
        new CreateTaskCommand(title, description, tags, dueDate, priority)
    );

    AnsiConsole.MarkupLine($"[green]✓[/] Task created with ID: {taskId}");
}

void DisplayTasks(IEnumerable<TaskItem> tasks)
{
    if (!tasks.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No tasks found.[/]");
        return;
    }

    var taskList = tasks.ToList();
    var taskDict = new Dictionary<string, Guid>();
    var choices = new List<string>();

    foreach (var task in taskList)
    {
        var displayText =
            $"{(task.IsCompleted ? "✓" : "○")} {task.Title.EscapeMarkup()} ({task.Priority})";
        choices.Add(displayText);
        taskDict[displayText] = task.Id;
    }

    var selectedTask = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select a task to copy its ID:")
            .PageSize(10)
            .AddChoices(choices)
    );

    var selectedId = taskDict[selectedTask];

    AnsiConsole.MarkupLine($"\n[green]Task ID:[/] [yellow]{selectedId}[/]\n");
}

async Task ListAllTasks(IMediator mediator)
{
    var tasks = await mediator.Send(new GetAllTasksQuery());

    DisplayTasks(tasks);
}

async Task ViewTaskById(IMediator mediator)
{
    var taskId = AnsiConsole.Ask<Guid>("Enter Task ID:");

    var task = await mediator.Send(new GetTaskByIdQuery(taskId));

    if (task == null)
    {
        AnsiConsole.MarkupLine("[red]✗[/] Task not found.");
        return;
    }

    var panel = new Panel(
        new Markup(
            $"""
            [bold]Title:[/] {task.Title}
            [bold]Description:[/] {task.Description ?? "N/A"}
            [bold]Priority:[/] {task.Priority}
            [bold]Status:[/] {(
                task.IsCompleted ? "[green]Completed ✓[/]" : "[grey]Incomplete ○[/]"
            )}
            [bold]Tags:[/] {(task.Tags.Any() ? string.Join(", ", task.Tags) : "None")}
            [bold]Due Date:[/] {task.DueDate?.ToString("dd/MM/yyyy") ?? "N/A"}
            [bold]Created At:[/] {task.CreatedAt:dd/MM/yyyy HH:mm}
            """
        )
    )
    {
        Header = new PanelHeader($"Task: {taskId}"),
        Border = BoxBorder.Rounded,
    };

    AnsiConsole.Write(panel);
}

async Task CompleteTask(IMediator mediator)
{
    var taskId = AnsiConsole.Ask<Guid>("Enter Task ID to complete:");

    var result = await mediator.Send(new CompleteTaskCommand(taskId));
    if (result.IsSuccess)
        AnsiConsole.MarkupLine("[green]✓[/] Task marked as completed.");
    else
        AnsiConsole.MarkupLine($"[red]✗[/] {result.Errors[0].Message}");
}

async Task UpdateTask(IMediator mediator)
{
    var taskId = AnsiConsole.Ask<Guid>("Enter Task ID to update:");

    var task = await mediator.Send(new GetTaskByIdQuery(taskId));
    if (task == null)
    {
        AnsiConsole.MarkupLine("[red]✗[/] Task not found.");
        return;
    }

    AnsiConsole.MarkupLine(
        $"[dim]Current values shown in brackets. Press Enter to keep current value.[/]"
    );

    var title = AnsiConsole.Confirm("Update title?")
        ? AnsiConsole.Ask<string>($"New Title ({task.Title.EscapeMarkup()}):", task.Title)
        : task.Title;

    var description = AnsiConsole.Confirm("Update description?")
        ? AnsiConsole.Ask<string>(
            $"New Description ({(task.Description ?? "N/A").EscapeMarkup()}):",
            task.Description ?? ""
        )
        : task.Description;

    var priority = AnsiConsole.Confirm("Update priority?")
        ? AnsiConsole.Prompt(
            new SelectionPrompt<TaskPriority>()
                .Title($"Select Priority (Current: {task.Priority}):")
                .AddChoices(Enum.GetValues<TaskPriority>())
        )
        : task.Priority;

    var tags = task.Tags;
    if (AnsiConsole.Confirm("Update tags?"))
    {
        var tagsDisplay = task.Tags.Any() ? string.Join(", ", task.Tags) : "None";
        var tagInput = AnsiConsole.Ask<string>(
            $"Enter tags (comma-separated) ({tagsDisplay.EscapeMarkup()}):",
            string.Join(", ", task.Tags)
        );
        tags = tagInput.Split(',').Select(t => t.Trim()).ToList();
    }

    var dueDate = task.DueDate;
    if (AnsiConsole.Confirm("Update due date?"))
    {
        var dueDateDisplay = task.DueDate?.ToString("dd/MM/yyyy") ?? "N/A";
        dueDate = AnsiConsole.Ask<DateTime?>($"New Due Date ({dueDateDisplay}):");
    }

    var result = await mediator.Send(
        new UpdateTaskCommand(taskId, title, description, tags, dueDate, priority)
    );
    if (result.IsSuccess)
        AnsiConsole.MarkupLine("[green]✓[/] Task updated successfully.");
    else
        AnsiConsole.MarkupLine($"[red]✗[/] {result.Errors[0].Message}");
}

async Task DeleteTask(IMediator mediator)
{
    var taskId = AnsiConsole.Ask<Guid>("Enter Task ID to delete:");

    var confirm = AnsiConsole.Confirm($"Are you sure you want to delete task {taskId}?");
    if (!confirm)
    {
        AnsiConsole.MarkupLine("[yellow]Delete cancelled.[/]");
        return;
    }

    await mediator.Send(new DeleteTaskCommand(taskId));
    AnsiConsole.MarkupLine("[green]✓[/] Task deleted successfully.");
}

async Task SearchTasks(IMediator mediator)
{
    var searchTerm = AnsiConsole.Ask<string>("Enter search term:");

    var result = await mediator.Send(new SearchTasksQuery(searchTerm));

    DisplayTasks(result);
}

async Task FilterTasks(IMediator mediator)
{
    var title = AnsiConsole.Confirm("Filter title?")
        ? AnsiConsole.Ask<string>("Enter Title:")
        : null;

    var description = AnsiConsole.Confirm("Filter description?")
        ? AnsiConsole.Ask<string>("Enter Description:")
        : null;

    bool? isCompleted = AnsiConsole.Confirm("Filter isCompleted?")
        ? AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Enter isCompleted:")
                .AddChoices(new[] { true, false })
        )
        : null;

    TaskPriority? priority = AnsiConsole.Confirm("Filter priority?")
        ? AnsiConsole.Prompt(
            new SelectionPrompt<TaskPriority>()
                .Title("Enter priority:")
                .AddChoices(Enum.GetValues<TaskPriority>())
        )
        : null;

    List<string>? tags = null;
    if (AnsiConsole.Confirm("Filter tags?"))
    {
        var tagInput = AnsiConsole.Ask<string>("Enter tags (comma-separated):");
        tags = tagInput.Split(',').Select(t => t.Trim()).ToList();
    }

    DateTime? dueBefore = null;
    if (AnsiConsole.Confirm("Filter due before date?"))
    {
        var dueDateDisplay = DateTime.Now.ToString("dd/MM/yyyy");
        dueBefore = AnsiConsole.Ask<DateTime?>($"Enter Latest Date ({dueDateDisplay}):");
    }

    DateTime? dueAfter = null;
    if (AnsiConsole.Confirm("Filter due after date?"))
    {
        var dueDateDisplay = DateTime.Now.ToString("dd/MM/yyyy");
        dueAfter = AnsiConsole.Ask<DateTime?>($"Enter Earliest Date ({dueDateDisplay}):");
    }

    TaskSortField? sortField = null;
    if (AnsiConsole.Confirm("Sort by field?"))
    {
        sortField = AnsiConsole.Prompt(
            new SelectionPrompt<TaskSortField>()
                .Title("Enter Sort Field:")
                .AddChoices(Enum.GetValues<TaskSortField>())
        );
    }

    bool descending = sortField is not null
        ? AnsiConsole.Confirm("Sort in descending order?")
        : false;

    var result = await mediator.Send(
        new FilterTasksQuery(
            title,
            description,
            isCompleted,
            priority,
            tags,
            dueBefore,
            dueAfter,
            sortField,
            descending
        )
    );

    DisplayTasks(result);
}

async Task ExportTasks(IMediator mediator)
{
    var tasks = await mediator.Send(new GetAllTasksQuery());

    if (!tasks.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No tasks found.[/]");
        return;
    }

    string? selectedFormat = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select a format:")
            .AddChoices(new[] { "CSV", "JSON", "Markdown" })
    );

    if (string.IsNullOrEmpty(selectedFormat))
    {
        AnsiConsole.MarkupLine("[yellow]Export cancelled.[/]");
        return;
    }

    var fileName = AnsiConsole.Ask<string>("Enter a file name:");
    var service = new ExportService(fileName);

    IExportStrategy exportStrategy = selectedFormat switch
    {
        "CSV" => new CsvExportStrategy(),
        "JSON" => new JsonExportStrategy(),
        "Markdown" => new MarkdownExportStrategy(),
        _ => throw new ArgumentOutOfRangeException(),
    };

    await service.ExportTasksAysnc(tasks, exportStrategy, CancellationToken.None);

    AnsiConsole.MarkupLine("[green]✓[/] Tasks exported successfully.");
}
