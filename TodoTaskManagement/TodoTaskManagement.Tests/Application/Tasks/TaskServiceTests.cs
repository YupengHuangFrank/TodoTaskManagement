using NSubstitute;
using NUnit.Framework;
using TodoTaskManagement.Application.Tasks;
using TodoTaskManagement.Infrastructure.Tasks;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Tests.Application.Tasks;

[TestFixture]
public class TaskServiceTests
{
    private ITaskRepository _taskRepository = null!;
    private TaskService _taskService = null!;

    [SetUp]
    public void SetUp()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _taskService = new TaskService(_taskRepository);
    }

    private static DomainTask MakeTask(Guid userId, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = userId,
        Title = "Test Task",
        Status = DomainTaskStatus.Todo,
        CreatedAt = DateTime.UtcNow
    };

    // --- GetTasksAsync ---

    [Test]
    public async Task GetTasksAsync_DelegatesToRepository()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<DomainTask> { MakeTask(userId) };
        _taskRepository.GetByUserIdAsync(userId, false).Returns(tasks);

        var result = await _taskService.GetTasksAsync(userId, false);

        Assert.That(result, Is.EqualTo(tasks));
        await _taskRepository.Received(1).GetByUserIdAsync(userId, false);
    }

    [Test]
    public async Task GetTasksAsync_ArchivedTrue_PassesThroughToRepository()
    {
        var userId = Guid.NewGuid();
        _taskRepository.GetByUserIdAsync(userId, true).Returns(new List<DomainTask>());

        await _taskService.GetTasksAsync(userId, true);

        await _taskRepository.Received(1).GetByUserIdAsync(userId, true);
    }

    // --- CreateTaskAsync ---

    [Test]
    public async Task CreateTaskAsync_AddsTaskAndReturnsIt()
    {
        var userId = Guid.NewGuid();
        var task = MakeTask(userId);

        var result = await _taskService.CreateTaskAsync(task);

        await _taskRepository.Received(1).AddAsync(task);
        Assert.That(result, Is.SameAs(task));
    }

    // --- UpdateTaskAsync ---

    [Test]
    public async Task UpdateTaskAsync_ValidOwnership_UpdatesFieldsAndSaves()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var existingTask = MakeTask(userId, taskId);
        var updates = new DomainTask
        {
            Id = taskId,
            UserId = userId,
            Title = "Updated Title",
            Description = "New desc",
            DueDate = DateTime.UtcNow.AddDays(3),
            Status = DomainTaskStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        var result = await _taskService.UpdateTaskAsync(userId, taskId, updates);

        Assert.That(result.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("New desc"));
        Assert.That(result.Status, Is.EqualTo(DomainTaskStatus.InProgress));
        await _taskRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public void UpdateTaskAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        _taskRepository.GetByIdAsync(taskId).Returns((DomainTask?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _taskService.UpdateTaskAsync(userId, taskId, MakeTask(userId)));
    }

    [Test]
    public void UpdateTaskAsync_TaskBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = MakeTask(ownerId, taskId);

        _taskRepository.GetByIdAsync(taskId).Returns(task);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _taskService.UpdateTaskAsync(requestingUserId, taskId, MakeTask(requestingUserId)));
    }

    // --- DeleteTaskAsync ---

    [Test]
    public async Task DeleteTaskAsync_ValidOwnership_DeletesTask()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = MakeTask(userId, taskId);

        _taskRepository.GetByIdAsync(taskId).Returns(task);

        await _taskService.DeleteTaskAsync(userId, taskId);

        await _taskRepository.Received(1).DeleteAsync(task);
    }

    [Test]
    public void DeleteTaskAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        _taskRepository.GetByIdAsync(taskId).Returns((DomainTask?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _taskService.DeleteTaskAsync(userId, taskId));
    }

    [Test]
    public void DeleteTaskAsync_TaskBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = MakeTask(ownerId, taskId);

        _taskRepository.GetByIdAsync(taskId).Returns(task);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _taskService.DeleteTaskAsync(requestingUserId, taskId));
    }

    // --- ArchiveAllDoneAsync ---

    [Test]
    public async Task ArchiveAllDoneAsync_DelegatesToRepository()
    {
        var userId = Guid.NewGuid();

        await _taskService.ArchiveAllDoneAsync(userId);

        await _taskRepository.Received(1).ArchiveAllDoneAsync(userId);
    }
}
