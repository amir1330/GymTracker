using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;
using GymTracker.Services;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ChartService _chartService;

    public DashboardController(GymDbContext context, UserManager<User> userManager, ChartService chartService)
    {
        _context = context;
        _userManager = userManager;
        _chartService = chartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var charts = await _context.DashboardCharts
            .Where(c => c.UserId == userId)
            .Include(c => c.Exercise)
            .OrderBy(c => c.Position)
            .ToListAsync();

        var chartDtos = new List<object>();

        foreach (var chart in charts)
        {
            var data = await ComputeChartData(userId, chart);
            chartDtos.Add(new
            {
                chart.Id,
                chart.Label,
                chart.Metric,
                chart.ExerciseId,
                exerciseName = chart.Exercise?.Name,
                chart.Period,
                chart.ChartType,
                chart.Position,
                data
            });
        }

        return Ok(chartDtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDashboardChartRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var chartCount = await _context.DashboardCharts.CountAsync(c => c.UserId == userId);
        if (chartCount >= 20)
        {
            return BadRequest(new { message = "Maximum 20 charts per dashboard" });
        }

        if (RequiresExercise(request.Metric) && !request.ExerciseId.HasValue)
        {
            return BadRequest(new { message = $"Exercise is required for metric '{request.Metric}'" });
        }

        if (request.ExerciseId.HasValue)
        {
            var exerciseExists = await _context.Exercises.AnyAsync(e => e.Id == request.ExerciseId.Value);
            if (!exerciseExists)
            {
                return BadRequest(new { message = "Exercise not found" });
            }
        }

        var chart = new DashboardChart
        {
            UserId = userId,
            Label = request.Label,
            Metric = request.Metric,
            ExerciseId = request.ExerciseId,
            Period = request.Period,
            ChartType = request.ChartType,
            Position = chartCount
        };

        _context.DashboardCharts.Add(chart);
        await _context.SaveChangesAsync();

        var data = await ComputeChartData(userId, chart);

        return CreatedAtAction(nameof(GetAll), new
        {
            chart.Id,
            chart.Label,
            chart.Metric,
            chart.ExerciseId,
            chart.Period,
            chart.ChartType,
            chart.Position,
            data
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDashboardChartRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var chart = await _context.DashboardCharts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (chart == null) return NotFound();

        if (RequiresExercise(request.Metric) && !request.ExerciseId.HasValue)
        {
            return BadRequest(new { message = $"Exercise is required for metric '{request.Metric}'" });
        }

        chart.Label = request.Label;
        chart.Metric = request.Metric;
        chart.ExerciseId = request.ExerciseId;
        chart.Period = request.Period;
        chart.ChartType = request.ChartType;

        await _context.SaveChangesAsync();

        return Ok(new { chart.Id, chart.Label, chart.Metric, chart.ExerciseId, chart.Period, chart.ChartType, chart.Position });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var chart = await _context.DashboardCharts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (chart == null) return NotFound();

        _context.DashboardCharts.Remove(chart);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderRequest> items)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        foreach (var item in items)
        {
            var chart = await _context.DashboardCharts
                .FirstOrDefaultAsync(c => c.Id == item.Id && c.UserId == userId);
            if (chart != null)
            {
                chart.Position = item.Position;
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private bool RequiresExercise(string metric)
    {
        return metric != "bodyWeight" && metric != "frequency" && metric != "volume";
    }

    private async Task<object> ComputeChartData(int userId, DashboardChart chart)
    {
        var workouts = await _chartService.GetWorkoutsForChart(userId, chart.Period, chart.ExerciseId);
        var points = _chartService.ComputePoints(workouts, chart.Metric);
        var summary = _chartService.ComputeSummary(points);

        return new { points, summary };
    }
}

public class CreateDashboardChartRequest
{
    public string Label { get; set; } = string.Empty;
    public string Metric { get; set; } = "weight";
    public int? ExerciseId { get; set; }
    public string Period { get; set; } = "30d";
    public string ChartType { get; set; } = "line";
}

public class UpdateDashboardChartRequest
{
    public string Label { get; set; } = string.Empty;
    public string Metric { get; set; } = "weight";
    public int? ExerciseId { get; set; }
    public string Period { get; set; } = "30d";
    public string ChartType { get; set; } = "line";
}

public class ReorderRequest
{
    public int Id { get; set; }
    public int Position { get; set; }
}
