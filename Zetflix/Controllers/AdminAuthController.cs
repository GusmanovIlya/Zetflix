using Microsoft.AspNetCore.Mvc;
using Zetflix.Services;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _authService;

    public AdminAuthController(IAdminAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AdminSessionDto>> Login(AdminLoginDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var session = await _authService.LoginAsync(request.Email.Trim(), request.Password);

        if (session is null)
        {
            return Unauthorized("Invalid admin credentials.");
        }

        return Ok(ToDto(session));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(token))
        {
            _authService.Logout(token);
        }

        return NoContent();
    }

    [HttpGet("session")]
    public IActionResult Session()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(token) || !_authService.IsValidToken(token))
        {
            return Unauthorized();
        }

        return Ok(new { authenticated = true });
    }

    private static AdminSessionDto ToDto(AdminSession session)
    {
        return new AdminSessionDto(
            session.Token,
            session.UserId,
            session.UserName,
            session.Email,
            session.ExpiresAtUtc
        );
    }
}

public sealed record AdminLoginDto(string Email, string Password);

public sealed record AdminSessionDto(
    string Token,
    int UserId,
    string UserName,
    string Email,
    DateTime ExpiresAtUtc
);
