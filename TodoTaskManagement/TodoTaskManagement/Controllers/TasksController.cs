using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoTaskManagement.Application.Tasks;
using TodoTaskManagement.Mapping;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] bool archived = false)
    {
        var tasks = await taskService.GetTasksAsync(GetUserId(), archived);
        return Ok(tasks.Select(t => t.ToApi()));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskApi body)
    {
        var task = await taskService.CreateTaskAsync(body.ToDomain(GetUserId()));
        return CreatedAtAction(null, task.ToApi());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskApi body)
    {
        var updated = await taskService.UpdateTaskAsync(GetUserId(), id, body.ToDomain(GetUserId()));
        return Ok(updated.ToApi());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        await taskService.DeleteTaskAsync(GetUserId(), id);
        return NoContent();
    }

    [HttpPost("archive-all")]
    public async Task<IActionResult> ArchiveAll()
    {
        await taskService.ArchiveAllDoneAsync(GetUserId());
        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
