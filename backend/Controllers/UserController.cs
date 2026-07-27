using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.User;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;

    public UserController(UserService userService, UserManager<User> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetUserByIdAsync(int.Parse(_userManager.GetUserId(User)!));
        if (user == null) return NotFound();

        var settings = await _userService.GetSettingsAsync(user.Id);

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
        var user = await _userService.GetUserByIdAsync(int.Parse(_userManager.GetUserId(User)!));
        if (user == null) return NotFound();

        var success = await _userService.UpdateProfileAsync(user, request.Weight, request.Height);
        if (!success) return BadRequest();

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
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var success = await _userService.UpdateSettingsAsync(userId, request.RestTimerEnabled, request.DefaultRestTimeSeconds, request.Theme);
        if (!success) return BadRequest();

        return Ok(new { message = "Settings updated" });
    }
}
