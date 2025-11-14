using MongoDB.Driver;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly MongoDbContext _ctx;
    public TaskRepository(MongoDbContext ctx) => _ctx = ctx;

    public async System.Threading.Tasks.Task<Models.TaskItem?> GetByIdAsync(string id) =>
        await _ctx.Tasks.Find(t => t.Id == id).FirstOrDefaultAsync();

    public async System.Threading.Tasks.Task<List<Models.TaskItem>> GetAllAsync(string? status, string? assigneeId, DateTime? start, DateTime? end)
    {
        var filter = Builders<Models.TaskItem>.Filter.Empty;
        if (!string.IsNullOrEmpty(status))
            filter &= Builders<Models.TaskItem>.Filter.Eq(t => t.Status, status);
        if (!string.IsNullOrEmpty(assigneeId))
            filter &= Builders<Models.TaskItem>.Filter.Eq(t => t.AssigneeId, assigneeId);
        if (start.HasValue)
            filter &= Builders<Models.TaskItem>.Filter.Gte(t => t.CreatedAt, start.Value);
        if (end.HasValue)
            filter &= Builders<Models.TaskItem>.Filter.Lte(t => t.CreatedAt, end.Value);

        return await _ctx.Tasks.Find(filter).ToListAsync();
    }

    public async System.Threading.Tasks.Task AddAsync(Models.TaskItem task) =>
        await _ctx.Tasks.InsertOneAsync(task);

    public async System.Threading.Tasks.Task UpdateAsync(Models.TaskItem task) =>
        await _ctx.Tasks.ReplaceOneAsync(t => t.Id == task.Id, task);

    public async System.Threading.Tasks.Task DeleteAsync(string id) =>
        await _ctx.Tasks.DeleteOneAsync(t => t.Id == id);
}