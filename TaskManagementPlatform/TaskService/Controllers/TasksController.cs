using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;

namespace TaskService.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _svc;

    public TasksController(ITaskService svc) => _svc = svc;

    private string GetUserIdFromToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new Exception("Missing or invalid token");

        var token = authHeader["Bearer ".Length..].Trim();
        var parts = token.Split('_');
        if (parts.Length != 2)
            throw new Exception("Invalid token format");

        return parts[0]; // userId
    }

    [HttpGet("{id}")]
    public async System.Threading.Tasks.Task<ActionResult<TaskDto>> Get(string id)
    {
        var task = await _svc.GetTaskAsync(id);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskDto>>> List(
        [FromQuery] string? status,
        [FromQuery] string? assigneeId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
    {
        return Ok(await _svc.ListTasksAsync(status, assigneeId, start, end));
    }

    [HttpPost]
    public async System.Threading.Tasks.Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest req)
    {
        string performerId;
        try
        {
            performerId = GetUserIdFromToken();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        var task = await _svc.CreateTaskAsync(req, performerId);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(string id, [FromBody] UpdateTaskRequest req)
    {
        string performerId;
        string token;
        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest("Missing or invalid token");

            token = authHeader["Bearer ".Length..].Trim();
            var parts = token.Split('_');
            if (parts.Length != 2)
                return BadRequest("Invalid token format");

            performerId = parts[0];
        }
        catch
        {
            return BadRequest("Invalid token");
        }

        await _svc.UpdateTaskAsync(id, req, performerId, token);
        return Ok("Task updated!");
    }

    [HttpDelete("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(string id)
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return BadRequest("Missing or invalid token");

        var token = authHeader["Bearer ".Length..].Trim();
        try
        {
            await _svc.DeleteTaskAsync(id, token);
            return Ok("Task deleted Successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet("sla-breaches")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskDto>>> SlaBreaches()
    {
        return Ok(await _svc.GetSlaBreachesAsync());
    }
}