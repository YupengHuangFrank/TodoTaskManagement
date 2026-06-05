using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TodoTaskManagement.Domain.Users;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;

namespace TodoTaskManagement.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DomainTask> Tasks => Set<DomainTask>();

    // Write side is a no-op — UTC is enforced at the API boundary.
    // Read side retagging is necessary because SQLite stores datetimes as plain text and
    // EF Core reads them back as DateTimeKind.Unspecified.
    private static readonly ValueConverter<DateTime, DateTime> _utcConverter = new(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> _utcNullableConverter = new(
        v => v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<DomainTask>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired();
            e.Property(t => t.Status).HasConversion<int>();
            e.Property(t => t.IsArchived).HasDefaultValue(false);
            e.Property(t => t.CreatedAt).IsRequired().HasConversion(_utcConverter);
            e.Property(t => t.DueDate).HasConversion(_utcNullableConverter);
            e.HasOne<User>()
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
