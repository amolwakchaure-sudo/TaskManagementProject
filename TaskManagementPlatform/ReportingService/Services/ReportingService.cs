using Microsoft.Extensions.Options;
using ReportingService.Models;
using ReportingService.Settings;
using System.Net.Http.Headers;

namespace ReportingService.Services;

public class ReportingService : IReportingService
{
    private readonly HttpClient _http;
    private readonly TaskServiceSettings _settings;

    public ReportingService(HttpClient http, IOptions<TaskServiceSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    private void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async System.Threading.Tasks.Task<ReportSummary> GetSummaryReportAsync(string token)
    {
        SetToken(token);
        var tasks = await _http.GetFromJsonAsync<List<TaskDto>>($"{_settings.BaseUrl}/api/tasks")
                    ?? new List<TaskDto>();

        var now = DateTime.UtcNow;
        var breaches = tasks.Count(t => t.DueDate.HasValue && t.DueDate < now && t.Status != "Completed");

        var byUser = tasks.GroupBy(t => t.AssigneeId).ToDictionary(g => g.Key, g => g.Count());
        var byStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.Count());

        return new ReportSummary
        {
            TotalTasks = tasks.Count,
            OpenTasks = tasks.Count(t => t.Status != "Completed"),
            CompletedTasks = tasks.Count(t => t.Status == "Completed"),
            SlaBreaches = breaches,
            TasksByUser = byUser,
            TasksByStatus = byStatus
        };
    }

    public async System.Threading.Tasks.Task<List<TaskDto>> GetSlaBreachesAsync(string token)
    {
        SetToken(token);
        return await _http.GetFromJsonAsync<List<TaskDto>>($"{_settings.BaseUrl}/api/tasks/sla-breaches")
               ?? new List<TaskDto>();
    }

    public async System.Threading.Tasks.Task<List<TaskDto>> GetTasksByUserAsync(string userId, string token)
    {
        SetToken(token);
        var allTasks = await _http.GetFromJsonAsync<List<TaskDto>>($"{_settings.BaseUrl}/api/tasks")
                       ?? new List<TaskDto>();
        return allTasks.Where(t => t.AssigneeId == userId).ToList();
    }
}