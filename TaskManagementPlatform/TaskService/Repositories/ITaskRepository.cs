namespace TaskService.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<Models.TaskItem?> GetByIdAsync(string id);
    System.Threading.Tasks.Task<List<Models.TaskItem>> GetAllAsync(string? status, string? assigneeId, DateTime? start, DateTime? end);
    System.Threading.Tasks.Task AddAsync(Models.TaskItem task);
    System.Threading.Tasks.Task UpdateAsync(Models.TaskItem task);
    System.Threading.Tasks.Task DeleteAsync(string id);
}