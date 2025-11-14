using ReportingService.Models;

namespace ReportingService.Services;

public interface IReportingService
{
    System.Threading.Tasks.Task<ReportSummary> GetSummaryReportAsync(string token);
    System.Threading.Tasks.Task<List<TaskDto>> GetSlaBreachesAsync(string token);
    System.Threading.Tasks.Task<List<TaskDto>> GetTasksByUserAsync(string userId, string token);
}