using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using TodoTaskManagement.Application.Tasks;
using TodoTaskManagement.Controllers;
using TodoTaskManagement.Models;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Tests.Api.Controllers;

[TestFixture]
public class TasksControllerTests
{
    private ITaskService _taskService = null!;

    [SetUp]
    public void SetUp()
    {
        _taskService = Substitute.For<ITaskService>();
    }

    private TasksController CreateController(Guid userId)
    {
        var controller = new TasksController(_taskService);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static DomainTask MakeTask(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = "Test",
        Status = DomainTaskStatus.Todo,
        CreatedAt = DateTime.UtcNow
    };

    // --- GetTasks ---

    [Test]
    public async Task GetTasks_Returns200WithMappedTasks()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<DomainTask> { MakeTask(userId), MakeTask(userId) };
        _taskService.GetTasksAsync(userId, false).Returns(tasks);

        var result = (OkObjectResult)await CreateController(userId).GetTasks(archived: false);

        Assert.That(result.StatusCode, Is.EqualTo(200));
        var items = (result.Value as IEnumerable<TaskApi>)?.ToList();
        Assert.That(items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetTasks_CallsServiceWithCorrectUserIdAndArchivedFlag()
    {
        var userId = Guid.NewGuid();
        _taskService.GetTasksAsync(userId, true).Returns(new List<DomainTask>());

        await CreateController(userId).GetTasks(archived: true);

        await _taskService.Received(1).GetTasksAsync(userId, true);
    }

    // --- CreateTask ---

    [Test]
    public async Task CreateTask_Returns201WithCreatedTask()
    {
        var userId = Guid.NewGuid();
        var body = new TaskApi { Title = "New task", Status = DomainTaskStatus.Todo };
        _taskService.CreateTaskAsync(Arg.Any<DomainTask>()).Returns(callInfo => callInfo.Arg<DomainTask>());

        var result = (CreatedAtActionResult)await CreateController(userId).CreateTask(body);

        Assert.That(result.StatusCode, Is.EqualTo(201));
        Assert.That(result.Value, Is.InstanceOf<TaskApi>());
        await _taskService.Received(1).CreateTaskAsync(Arg.Is<DomainTask>(t => t.UserId == userId));
    }

    // --- UpdateTask ---

    [Test]
    public async Task UpdateTask_Returns200WithUpdatedTask()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var updatedTask = MakeTask(userId);
        updatedTask.Id = taskId;
        var body = new TaskApi { Title = "Updated", Status = DomainTaskStatus.InProgress };

        _taskService.UpdateTaskAsync(userId, taskId, Arg.Any<DomainTask>()).Returns(updatedTask);

        var result = (OkObjectResult)await CreateController(userId).UpdateTask(taskId, body);

        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.InstanceOf<TaskApi>());
        await _taskService.Received(1).UpdateTaskAsync(userId, taskId, Arg.Any<DomainTask>());
    }

    // --- DeleteTask ---

    [Test]
    public async Task DeleteTask_Returns204()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var result = await CreateController(userId).DeleteTask(taskId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        await _taskService.Received(1).DeleteTaskAsync(userId, taskId);
    }

    // --- ArchiveAll ---

    [Test]
    public async Task ArchiveAll_Returns204()
    {
        var userId = Guid.NewGuid();

        var result = await CreateController(userId).ArchiveAll();

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        await _taskService.Received(1).ArchiveAllDoneAsync(userId);
    }
}
