using TodoTaskManagement.Infrastructure.Tasks;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Application.Tasks;

public interface ITaskService
{
    Task<List<DomainTask>> GetTasksAsync(Guid userId, bool archived);
    Task<DomainTask> CreateTaskAsync(DomainTask task);
    Task<DomainTask> UpdateTaskAsync(Guid userId, Guid taskId, DomainTask updates);
    Task DeleteTaskAsync(Guid userId, Guid taskId);
    Task ArchiveAllDoneAsync(Guid userId);
}

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<List<DomainTask>> GetTasksAsync(Guid userId, bool archived) =>
        await taskRepository.GetByUserIdAsync(userId, archived);

    public async Task<DomainTask> CreateTaskAsync(DomainTask task)
    {
        await taskRepository.AddAsync(task);
        return task;
    }

    public async Task<DomainTask> UpdateTaskAsync(Guid userId, Guid taskId, DomainTask updates)
    {
        var task = await GetOwnedTaskAsync(userId, taskId);

        task.Title = updates.Title;
        task.Description = updates.Description;
        task.DueDate = updates.DueDate;
        task.Status = updates.Status;

        await taskRepository.SaveChangesAsync();
        return task;
    }

    public async Task DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var task = await GetOwnedTaskAsync(userId, taskId);
        await taskRepository.DeleteAsync(task);
    }

    public async Task ArchiveAllDoneAsync(Guid userId) =>
        await taskRepository.ArchiveAllDoneAsync(userId);

    private async Task<DomainTask> GetOwnedTaskAsync(Guid userId, Guid taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException($"Task not found.");

        if (task.UserId != userId)
            throw new KeyNotFoundException($"Task not found.");

        return task;
    }
}
