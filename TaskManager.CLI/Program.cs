using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TaskManager.Application.Commands;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Queries;
using TaskManager.Domain;
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
            .AddChoices(["Create Task", "List All Tasks", "Exit"])
    );

    switch (choice)
    {
        case "Create Task":
            await CreateTask(mediator);
            break;
        case "List All Tasks":
            await ListAllTasks(mediator);
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

async Task ListAllTasks(IMediator mediator)
{
    var tasks = await mediator.Send(new GetAllTasksQuery());

    if (!tasks.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No tasks found.[/]");
        return;
    }

    var table = new Table();
    table.AddColumn("Title");
    table.AddColumn("Description");
    table.AddColumn("Priority");
    table.AddColumn("Status");
    table.AddColumn("Tags");
    table.AddColumn("Due Date");

    table.Centered();

    foreach (var task in tasks)
    {
        table.AddRow(
            new Text(task.Title),
            new Text(task.Description ?? ""),
            new Text(task.Priority.ToString()).Justify(Justify.Center),
            new Markup(task.IsCompleted ? "[green]✓[/]" : "[grey]○[/]").Justify(Justify.Center),
            new Text(task.Tags.Any() ? string.Join(", ", task.Tags) : ""),
            new Text(task.DueDate?.ToString("dd/MM/yy") ?? "-")
        );
    }

    AnsiConsole.Write(table);
}
