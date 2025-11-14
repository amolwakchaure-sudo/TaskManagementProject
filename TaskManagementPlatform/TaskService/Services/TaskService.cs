using TaskService.Models;
using TaskService.Repositories;

namespace TaskService.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly IUserValidationService _userSvc;

    public TaskService(ITaskRepository repo, IUserValidationService userSvc)
    {
        _repo = repo;
        _userSvc = userSvc;
    }

    public async System.Threading.Tasks.Task<TaskDto?> GetTaskAsync(string id)
    {
        var task = await _repo.GetByIdAsync(id);
        return task == null ? null : Map(task);
    }

    public async System.Threading.Tasks.Task<List<TaskDto>> ListTasksAsync(string? status, string? assigneeId, DateTime? start, DateTime? end)
    {
        var tasks = await _repo.GetAllAsync(status, assigneeId, start, end);
        return tasks.Select(Map).ToList();
    }

    public async System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskRequest req, string performerId)
    {
        if (!await _userSvc.UserExistsAsync(req.AssigneeId, "dummy"))
            throw new Exception("Assignee user does not exist");

        var task = new Models.TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            AssigneeId = req.AssigneeId,
            DueDate = req.DueDate,
            ActivityLogs = new List<ActivityLog>
            {
                new() { Action = "Task Created", PerformedBy = performerId }
            }
        };
        await _repo.AddAsync(task);
        return Map(task);
    }

    public async System.Threading.Tasks.Task UpdateTaskAsync(string id, UpdateTaskRequest req, string performerId, string token)
    {
        var task = await _repo.GetByIdAsync(id) ?? throw new Exception("Task not found");

        if (req.AssigneeId != null && req.AssigneeId != task.AssigneeId)
        {
            if (!await _userSvc.UserExistsAsync(req.AssigneeId, token))
                throw new Exception("New assignee does not exist");
            task.AssigneeId = req.AssigneeId;
            task.ActivityLogs.Add(new ActivityLog { Action = "Assignee Changed", PerformedBy = performerId });
        }

        if (req.Status != null && req.Status != task.Status)
        {
            task.Status = req.Status;
            task.ActivityLogs.Add(new ActivityLog { Action = $"Status to {req.Status}", PerformedBy = performerId });
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(task);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(string id, string token)
    {
        var parts = token.Split('_');
        if (parts.Length != 2 || parts[1] != "Admin")
            throw new Exception("Only Admin can delete tasks");

        await _repo.DeleteAsync(id);
    }

    public async System.Threading.Tasks.Task<List<TaskDto>> GetSlaBreachesAsync()
    {
        var tasks = await _repo.GetAllAsync(null, null, null, null);
        var now = DateTime.UtcNow;
        return tasks
            .Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != "Completed")
            .Select(Map)
            .ToList();
    }

    private static TaskDto Map(Models.TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority,
        Status = t.Status,
        AssigneeId = t.AssigneeId,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        DueDate = t.DueDate,
        ActivityLogs = t.ActivityLogs
    };
}