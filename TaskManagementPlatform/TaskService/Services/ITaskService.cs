using TaskService.Models;

namespace TaskService.Services;

public interface ITaskService
{
    System.Threading.Tasks.Task<TaskDto?> GetTaskAsync(string id);
    System.Threading.Tasks.Task<List<TaskDto>> ListTasksAsync(string? status, string? assigneeId, DateTime? start, DateTime? end);
    System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskRequest req, string performerId);
    System.Threading.Tasks.Task UpdateTaskAsync(string id, UpdateTaskRequest req, string performerId, string token);
    System.Threading.Tasks.Task DeleteTaskAsync(string id, string token);
    System.Threading.Tasks.Task<List<TaskDto>> GetSlaBreachesAsync();
}