using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<UserDto?> GetUserAsync(string id)
    {
        var u = await _repo.GetByIdAsync(id);
        return u == null ? null : Map(u);
    }

    public async Task<IEnumerable<UserDto>> ListUsersAsync()
    {
        var users = await _repo.GetAllAsync();
        return users.Select(Map);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest req)
    {
        if (await _repo.GetByUsernameAsync(req.Username) != null)
            throw new Exception("Username already exists");

        var user = new User
        {
            Username = req.Username,
            PasswordHash = req.Password,
            Role = req.Role
        };
        await _repo.AddAsync(user);
        return Map(user);
    }

    public async Task UpdateUserAsync(string id, UserDto dto)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new Exception("User not found");
        u.Username = dto.Username;
        u.Role = dto.Role;
        await _repo.UpdateAsync(u);
    }

    public async Task DeleteUserAsync(string id)
    {
        await _repo.DeleteAsync(id);
    }

    private static UserDto Map(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}