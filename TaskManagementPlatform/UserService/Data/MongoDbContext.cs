using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserService.Models;

namespace UserService.Data;

public class MongoDbContext
{
    public IMongoCollection<User> Users { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        Users = db.GetCollection<User>("Users");
    }
}