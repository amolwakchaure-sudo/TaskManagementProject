using MongoDB.Driver;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _ctx;
    public UserRepository(MongoDbContext ctx) => _ctx = ctx;

    public async Task<User?> GetByIdAsync(string id) =>
        await _ctx.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _ctx.Users.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _ctx.Users.Find(_ => true).ToListAsync();

    public async Task AddAsync(User user) =>
        await _ctx.Users.InsertOneAsync(user);

    public async Task UpdateAsync(User user) =>
        await _ctx.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

    public async Task DeleteAsync(string id) =>
        await _ctx.Users.DeleteOneAsync(u => u.Id == id);

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        return await _ctx.Users
            .Find(u => u.Username == username && u.PasswordHash == password)
            .FirstOrDefaultAsync();
    }
}