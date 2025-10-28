using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kadikoy.DTOs;
using Kadikoy.Interfaces;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminPanelController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminPanelController> _logger;

    public AdminPanelController(IAuthService authService, ILogger<AdminPanelController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Admin panel login endpoint
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Geçersiz giriş bilgileri", errors = ModelState });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" });
            }

            // Check if user has Admin role
            if (!result.Roles.Contains("Admin"))
            {
                return Forbid("Bu alana erişim yetkiniz yok");
            }

            _logger.LogInformation("Admin user {Username} logged in successfully", loginDto.Username);

            return Ok(new
            {
                message = "Giriş başarılı",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login for user {Username}", loginDto.Username);
            return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu" });
        }
    }

    /// <summary>
    /// Test endpoint to verify admin authentication
    /// </summary>
    /// <returns>Success message</returns>
    [HttpGet("test")]
    [Authorize(Roles = "Admin")]
    public IActionResult Test()
    {
        var username = User.Identity?.Name;
        return Ok(new
        {
            message = "Admin panel erişimi başarılı",
            username = username,
            roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                               .Select(c => c.Value)
                               .ToList()
        });
    }

    /// <summary>
    /// Get current admin user information
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.Identity?.Name;
        var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                               .Select(c => c.Value)
                               .ToList();

        return Ok(new
        {
            userId = userId,
            username = username,
            roles = roles
        });
    }
}

