using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Data;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Auth;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtService _jwtService;
    private readonly GymDbContext _context;

    public AuthController(UserManager<User> userManager, JwtService jwtService, GymDbContext context)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters" });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email already registered" });
        }

        existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already taken" });
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            Weight = request.Weight,
            Height = request.Height
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        _context.UserSettings.Add(new UserSettings
        {
            UserId = user.Id,
            RestTimerEnabled = true,
            DefaultRestTimeSeconds = 90,
            Theme = "dark"
        });
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.UserName!, user.Email!);
        return Ok(new { token, userId = user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return BadRequest(new { message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(user.Id, user.UserName!, user.Email!);
        return Ok(new { token, userId = user.Id });
    }
}
