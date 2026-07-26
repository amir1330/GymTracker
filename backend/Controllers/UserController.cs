using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly GymDbContext _context;

    public UserController(UserManager<User> userManager, GymDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Weight,
            user.Height,
            Settings = settings != null ? new
            {
                settings.RestTimerEnabled,
                settings.DefaultRestTimeSeconds,
                settings.Theme
            } : null
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.Weight = request.Weight;
        user.Height = request.Height;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Weight,
            user.Height
        });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = user.Id,
                RestTimerEnabled = request.RestTimerEnabled ?? true,
                DefaultRestTimeSeconds = request.DefaultRestTimeSeconds ?? 90,
                Theme = request.Theme ?? "dark"
            };
            _context.UserSettings.Add(settings);
        }
        else
        {
            if (request.RestTimerEnabled.HasValue)
                settings.RestTimerEnabled = request.RestTimerEnabled.Value;
            if (request.DefaultRestTimeSeconds.HasValue)
                settings.DefaultRestTimeSeconds = request.DefaultRestTimeSeconds.Value;
            if (request.Theme != null)
                settings.Theme = request.Theme;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Settings updated" });
    }
}

public class UpdateProfileRequest
{
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
}

public class UpdateSettingsRequest
{
    public bool? RestTimerEnabled { get; set; }
    public int? DefaultRestTimeSeconds { get; set; }
    public string? Theme { get; set; }
}
