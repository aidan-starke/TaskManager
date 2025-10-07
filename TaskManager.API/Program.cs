using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Database
builder.Services.AddDbContext<TaskDbContext>(options => options.UseSqlite("Data Source=tasks.db"));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(TaskManager.Application.Commands.CreateTaskCommand).Assembly
    )
);

// Add Repository
builder.Services.AddScoped<ITaskRepository, SqliteTaskRepository>();

// Add CORS
var allowedOrigins = new List<string> { "http://localhost:5173" };

// Add custom origins from environment variable
var customOrigin = builder.Configuration["CORS_ORIGIN"];
if (!string.IsNullOrEmpty(customOrigin))
{
    allowedOrigins.Add(customOrigin);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins.ToArray()).AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
