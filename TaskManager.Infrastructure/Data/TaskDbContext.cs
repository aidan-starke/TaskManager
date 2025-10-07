using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);

            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.Property(e => e.Priority).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();

            entity.Property(e => e.DueDate);

            entity.Property(e => e.IsCompleted).IsRequired();

            // Store Tags as JSON
            entity
                .Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
    }
}
