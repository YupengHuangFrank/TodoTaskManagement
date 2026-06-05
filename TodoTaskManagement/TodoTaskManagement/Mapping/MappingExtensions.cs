using TodoTaskManagement.Models;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Mapping;

public static class MappingExtensions
{
    public static TaskApi ToApi(this DomainTask task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status,
        IsArchived = task.IsArchived,
        CreatedAt = task.CreatedAt
    };

    public static DomainTask ToDomain(this TaskApi api, Guid userId) => new()
    {
        Id = api.Id ?? Guid.NewGuid(),
        UserId = userId,
        Title = api.Title!,
        Description = api.Description,
        DueDate = api.DueDate,
        Status = api.Status!.Value,
        IsArchived = api.IsArchived ?? false,
        CreatedAt = DateTime.UtcNow
    };
}
