using Microsoft.AspNetCore.Mvc;
using GymTracker.Services;
using GymTracker.DTOs.Auth;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, token, userId, error) = await _authService.RegisterAsync(request);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { token, userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, userId, error) = await _authService.LoginAsync(
            request.Email, request.Password);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { token, userId });
    }
}
