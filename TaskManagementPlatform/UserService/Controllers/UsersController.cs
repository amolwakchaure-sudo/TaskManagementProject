using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService svc, ILogger<UsersController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(string id)
    {
        var user = await _svc.GetUserAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        return Ok(await _svc.ListUsersAsync());
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        var user = await _svc.CreateUserAsync(req);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserDto dto)
    {
        await _svc.UpdateUserAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var authHeader = Request.Headers["Authorization"].ToString();

        // 1. No token or wrong format
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return StatusCode(401, "Unauthorized: Token missing or invalid format. Use: Bearer {token}");

        var token = authHeader["Bearer ".Length..].Trim();
        var parts = token.Split('_');

        // 2. Token format invalid or not Admin
        if (parts.Length != 2 || parts[1] != "Admin")
            return StatusCode(403, "Forbidden: Only Admin can delete users");

        // 3. User not found
        var userToDelete = await _svc.GetUserAsync(id);
        if (userToDelete == null)
            return NotFound("User not found");

        // 4. Delete
        await _svc.DeleteUserAsync(id);
        return Ok("User Deleted Successfully!");
    }

    private string GetRoleFromToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return "Engineer";

        var token = authHeader["Bearer ".Length..];
        var parts = token.Split('_');
        return parts.Length == 2 ? parts[1] : "Engineer";
    }
}