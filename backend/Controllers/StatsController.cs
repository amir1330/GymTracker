using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Stats;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly ChartService _chartService;
    private readonly UserManager<User> _userManager;

    public StatsController(ChartService chartService, UserManager<User> userManager)
    {
        _chartService = chartService;
        _userManager = userManager;
    }

    [HttpPost("chart-data")]
    public async Task<IActionResult> GetChartData([FromBody] ChartDataRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var workouts = await _chartService.GetWorkoutsForChart(userId, request.Period, request.ExerciseId, request.Metric);
        var points = _chartService.ComputePoints(workouts, request.Metric);
        var summary = _chartService.ComputeSummary(points);

        return Ok(new { points, summary });
    }
}
