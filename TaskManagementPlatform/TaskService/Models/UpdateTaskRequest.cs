namespace TaskService.Models;

public class UpdateTaskRequest
{
    public string? Status { get; set; }
    public string? AssigneeId { get; set; }
}