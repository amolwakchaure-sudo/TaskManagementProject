namespace TaskService.Models;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string AssigneeId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}