namespace ReportingService.Models;

public class TaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssigneeId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}