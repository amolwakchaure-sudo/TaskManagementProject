using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(string id);
    Task<IEnumerable<UserDto>> ListUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(string id, UserDto userDto);
    Task DeleteUserAsync(string id);
}