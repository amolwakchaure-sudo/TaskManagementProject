namespace ReportingService.Models;

public class ReportSummary
{
    public int TotalTasks { get; set; }
    public int OpenTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int SlaBreaches { get; set; }
    public Dictionary<string, int> TasksByUser { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
}