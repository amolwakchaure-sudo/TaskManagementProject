using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using TaskService.Data;

namespace TaskService.Services;

public class UserValidationService : IUserValidationService
{
    private readonly HttpClient _http;
    private readonly UserServiceSettings _settings;

    public UserValidationService(HttpClient http, IOptions<UserServiceSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async System.Threading.Tasks.Task<bool> UserExistsAsync(string userId, string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _http.GetAsync($"{_settings.BaseUrl}/api/users/{userId}");
        return response.IsSuccessStatusCode;
    }
}