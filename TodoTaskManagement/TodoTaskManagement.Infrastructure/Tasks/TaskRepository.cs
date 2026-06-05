using Microsoft.EntityFrameworkCore;
using TodoTaskManagement.Infrastructure.Data;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Infrastructure.Tasks;

public interface ITaskRepository
{
    Task<List<DomainTask>> GetByUserIdAsync(Guid userId, bool archived);
    Task<DomainTask?> GetByIdAsync(Guid id);
    Task AddAsync(DomainTask task);
    Task SaveChangesAsync();
    Task DeleteAsync(DomainTask task);
    Task ArchiveAllDoneAsync(Guid userId);
}

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<List<DomainTask>> GetByUserIdAsync(Guid userId, bool archived) =>
        await context.Tasks
            .Where(t => t.UserId == userId && t.IsArchived == archived)
            .ToListAsync();

    public async Task<DomainTask?> GetByIdAsync(Guid id) =>
        await context.Tasks.FindAsync(id);

    public async Task AddAsync(DomainTask task)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        foreach (var entry in context.ChangeTracker.Entries<DomainTask>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Property(t => t.CreatedAt).CurrentValue = entry.Property(t => t.CreatedAt).OriginalValue;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(DomainTask task)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
    }

    public async Task ArchiveAllDoneAsync(Guid userId) =>
        await context.Tasks
            .Where(t => t.UserId == userId && t.Status == DomainTaskStatus.Done && !t.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsArchived, true));
}
