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

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var settings = await _userService.GetSettingsAsync(userId);
        if (settings == null) return NotFound();

        return Ok(new { theme = settings.Theme, language = settings.Language });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var success = await _userService.UpdateSettingsAsync(userId, request);
        if (!success) return BadRequest();

        return Ok(new { message = "Settings updated" });
    }
}
