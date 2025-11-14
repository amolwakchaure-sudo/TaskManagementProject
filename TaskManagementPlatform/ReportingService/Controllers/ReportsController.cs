using Microsoft.AspNetCore.Mvc;
using ReportingService.Models;
using ReportingService.Services;

namespace ReportingService.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _svc;

    public ReportsController(IReportingService svc) => _svc = svc;

    private string GetToken()
    {
        var auth = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
            throw new Exception("Missing or invalid token");
        return auth["Bearer ".Length..].Trim();
    }

    [HttpGet("summary")]
    public async System.Threading.Tasks.Task<ActionResult<ReportSummary>> Summary()
    {
        var token = GetToken();
        var report = await _svc.GetSummaryReportAsync(token);
        return Ok(report);
    }

    [HttpGet("sla-breaches")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskDto>>> SlaBreaches()
    {
        var token = GetToken();
        var tasks = await _svc.GetSlaBreachesAsync(token);
        return Ok(tasks);
    }

    [HttpGet("user/{userId}")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskDto>>> TasksByUser(string userId)
    {
        var token = GetToken();
        var tasks = await _svc.GetTasksByUserAsync(userId, token);
        return Ok(tasks);
    }
}