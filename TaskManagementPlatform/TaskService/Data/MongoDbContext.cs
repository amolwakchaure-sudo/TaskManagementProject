using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskService.Models;

namespace TaskService.Data;

public class MongoDbContext
{
    public IMongoCollection<Models.TaskItem> Tasks { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        Tasks = db.GetCollection<Models.TaskItem>("Tasks");
    }
}