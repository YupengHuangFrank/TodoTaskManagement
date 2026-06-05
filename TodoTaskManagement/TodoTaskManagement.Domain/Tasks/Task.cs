namespace TodoTaskManagement.Domain.Tasks;

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}

public class Task
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
