# Task Manager - A Clean Architecture Learning Project

A comprehensive C# task management application built as a learning project to explore modern software architecture patterns, CQRS, and C# best practices.

## Project Overview

This project is a fully-featured CLI task management system that demonstrates production-quality code with comprehensive test coverage. It was built through guided learning, where I worked hands-on with each component while being coached through architectural decisions and implementation details.

## How This Project Was Created

This project was built through an iterative, learning-focused approach:

1. **Planning Phase**: Started by discussing what concepts I wanted to learn and selecting a project that would incorporate multiple architectural patterns
2. **Guided Implementation**: Rather than having code written for me, I was guided through implementing each layer, understanding the "why" behind every decision
3. **Test-Driven Approach**: Wrote comprehensive tests for each feature, learning testing best practices along the way
4. **Refactoring & Review**: After implementing core features, conducted code reviews and refactoring sessions to improve code quality
5. **Pattern Implementation**: Added advanced patterns (Strategy, Result) to enhance the architecture

## What I Learned

### Architecture & Design Patterns

**Clean Architecture**

- Separation of concerns across five distinct layers
- Dependency flow from outer layers (CLI, Infrastructure) towards inner layers (Domain)
- How to structure projects for maintainability and testability

**CQRS (Command Query Responsibility Segregation)**

- Separating read operations (Queries) from write operations (Commands)
- Using MediatR for request/response pattern implementation
- Benefits of explicit command/query modeling

**Repository Pattern**

- Abstracting data access behind interfaces
- How repositories enable testing through mocking
- Lazy loading with `Lazy<T>` for performance optimization

**Strategy Pattern**

- Implementing different export formats (CSV, JSON, Markdown) with a common interface
- Runtime algorithm selection
- Open/Closed Principle in practice

**Result Pattern**

- Using FluentResults to handle errors explicitly without exceptions
- Performance benefits of avoiding exception-based control flow
- Making potential failures explicit in the type system

### Modern C# Features

**Primary Constructors**

- Using constructor parameters directly in class bodies
- When to use primary constructors vs traditional constructors
- Parameter capture considerations (CS9124)

**Records**

- Immutable data structures for Commands and Queries
- Value-based equality
- Concise syntax with positional parameters

**Pattern Matching**

- Switch expressions for clean, readable code
- Pattern matching with null checks (`is not null`)
- Type patterns and property patterns

**Nullable Reference Types**

- Explicit nullability annotations (`string?`, `List<string>?`)
- Handling null cases safely
- Compiler-enforced null safety

**Init-only Properties**

- Properties that can only be set during initialization
- Immutability patterns
- Creating immutable objects with object initializers

**Async/Await**

- Asynchronous programming patterns
- CancellationToken propagation
- Proper async method signatures

### Testing Practices

**xUnit Framework**

- Writing unit tests with Facts and Theories
- Test organization and naming conventions
- Test lifecycle management

**Moq (Mocking Framework)**

- Creating mock repositories for testing
- Verifying method calls with `Mock.Verify`
- Setting up mock return values with `Returns` and `ReturnsAsync`

**FluentAssertions**

- Readable, expressive test assertions
- Chaining assertions for complex validations
- Better error messages compared to standard assertions

**Test Organization**

- Base classes to reduce test duplication (`TaskCommandTestBase`, `TaskQueryTestBase`)
- Shared test data setup
- Integration vs unit testing approaches

### LINQ & Extension Methods

**Custom LINQ Extensions**

- Creating chainable extension methods
- Method chaining for clean, fluent APIs
- Extending `IEnumerable<T>` with domain-specific operations

**Advanced LINQ**

- `Where`, `OrderBy`, `Select`, `Any`, `Intersect`
- Deferred execution concepts
- Performance considerations with LINQ

### Dependency Injection

**Microsoft.Extensions.DependencyInjection**

- Service registration (`AddSingleton`, `AddTransient`, `AddScoped`)
- Service lifetime management
- Building and using service providers
- Integrating with MediatR

### Additional Concepts

**JSON Serialization**

- Using System.Text.Json for serialization/deserialization
- File I/O with proper stream disposal
- JSON file-based persistence

**CLI Development with Spectre.Console**

- Building interactive terminal UIs
- Selection prompts and user input
- Markup syntax for colored output
- Creating panels and formatted displays

## Project Structure

```
TaskManager/
├── TaskManager.Domain/              # Core business entities
│   ├── TaskItem.cs                  # Main task entity
│   ├── TaskPriority.cs              # Priority enumeration
│   └── TaskSortField.cs             # Sorting options
│
├── TaskManager.Application/         # Business logic layer
│   ├── Commands/                    # Write operations
│   │   ├── CreateTaskCommand.cs
│   │   ├── CompleteTaskCommand.cs
│   │   ├── UpdateTaskCommand.cs
│   │   └── DeleteTaskCommand.cs
│   ├── Queries/                     # Read operations
│   │   ├── GetAllTasksQuery.cs
│   │   ├── GetTaskByIdQuery.cs
│   │   ├── FilterTasksQuery.cs
│   │   └── SearchTasksQuery.cs
│   ├── Handlers/                    # Command/Query handlers
│   ├── Extensions/                  # Custom LINQ extensions
│   │   └── TaskItemExtensions.cs
│   └── Interfaces/                  # Application interfaces
│       ├── ITaskRepository.cs
│       └── IExportStrategy.cs
│
├── TaskManager.Infrastructure/      # External concerns
│   ├── Repositories/
│   │   └── JsonTaskRepository.cs    # JSON file-based storage
│   └── Export/                      # Export implementations
│       ├── CsvExportStrategy.cs
│       ├── JsonExportStrategy.cs
│       └── MarkdownExportStrategy.cs
│
├── TaskManager.CLI/                 # User interface
│   └── Program.cs                   # Interactive CLI menu
│
└── TaskManager.Tests/               # Comprehensive test suite
    ├── Commands/                    # Command handler tests
    ├── Queries/                     # Query handler tests
    ├── Extensions/                  # Extension method tests
    ├── Export/                      # Export strategy tests
    ├── Infrastructure/              # Repository tests
    └── TaskCommandTestBase.cs       # Shared test infrastructure
```

## Features

### Core Functionality

- **Create Tasks**: Add new tasks with title, description, priority, tags, and due dates
- **View Tasks**: List all tasks or view individual task details
- **Update Tasks**: Modify existing task properties
- **Complete Tasks**: Mark tasks as completed
- **Delete Tasks**: Remove tasks from the system

### Advanced Features

- **Filtering**: Filter tasks by title, description, status, priority, tags, and date ranges
- **Sorting**: Sort by title, priority, due date, creation date, or completion status
- **Search**: Full-text search across task titles and descriptions
- **Export**: Export tasks to CSV, JSON, or Markdown formats

### Technical Highlights

- **121 Passing Tests**: Comprehensive test coverage across all layers
- **Result Pattern**: Explicit error handling without exceptions
- **Lazy Loading**: Efficient resource usage with deferred initialization
- **Async Throughout**: Full async/await implementation with cancellation support

## Getting Started

### Prerequisites

- .NET 9.0 SDK

### Running the Application

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the CLI application
dotnet run --project TaskManager.CLI
```

### Example Usage

1. Launch the application and select "Create Task"
2. Enter task details (title, description, priority, tags, due date)
3. Use "Filter Tasks" to find specific tasks
4. Mark tasks complete, update details, or export to various formats

## Key Takeaways

This project taught me that:

1. **Architecture Matters**: Clean Architecture made the codebase easy to test, modify, and understand
2. **Patterns Solve Problems**: Each design pattern (CQRS, Repository, Strategy, Result) solved specific challenges
3. **Testing Enables Confidence**: With 121 tests, I can refactor fearlessly knowing functionality is preserved
4. **Modern C# is Powerful**: Features like records, primary constructors, and pattern matching make code concise and expressive
5. **Explicit is Better**: Result pattern and nullable reference types make potential issues visible at compile time
6. **Iterative Development Works**: Building incrementally with tests and reviews leads to higher quality code

## Future Enhancements

Potential areas for continued learning:

- Pagination for large task lists
- Input validation with FluentValidation
- Task recurrence and reminders
- Categories/projects for task organization
- Unit of Work pattern for transaction management
- Entity Framework Core for database persistence
- Web API with ASP.NET Core
- Authentication and multi-user support

## Statistics

- **43 C# source files**
- **121 unit tests** (all passing)
- **5 architectural layers**
- **13 CQRS operations** (6 queries, 7 commands)
- **3 export formats**
- **7 custom LINQ extensions**
- **5 sorting options**
- **Multiple filtering criteria**

## Conclusion

This project demonstrates production-quality C# development with modern patterns and practices. It serves as both a functional application and a comprehensive reference for Clean Architecture, CQRS, testing strategies, and contemporary C# features. The hands-on, guided approach to building this project provided deep understanding of not just _how_ to implement these patterns, but _why_ they matter and _when_ to use them.
