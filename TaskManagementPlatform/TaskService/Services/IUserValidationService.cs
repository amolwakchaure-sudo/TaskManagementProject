namespace TaskService.Services;

public interface IUserValidationService
{
    System.Threading.Tasks.Task<bool> UserExistsAsync(string userId, string token);
}