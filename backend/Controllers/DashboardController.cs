using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Dashboard;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly UserManager<User> _userManager;

    public DashboardController(DashboardService dashboardService, UserManager<User> userManager)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var charts = await _dashboardService.GetAllAsync(userId);
        var allChartData = await _dashboardService.GetAllChartDataAsync(userId, charts);

        var chartDtos = charts.Select(chart =>
        {
            var chartData = allChartData.FirstOrDefault(d => d.ChartId == chart.Id);
            return new DashboardChartResponse
            {
                Id = chart.Id,
                Label = chart.Label,
                Metric = chart.Metric,
                ExerciseId = chart.ExerciseId,
                ExerciseName = chart.Exercise?.Name,
                Period = chart.Period,
                ChartType = chart.ChartType,
                Position = chart.Position,
                Data = new ChartDataResponse
                {
                    Points = chartData?.Points.Select(p => new ChartDataPointResponse { Date = p.Date, Value = p.Value }).ToList() ?? new(),
                    Summary = chartData?.Summary != null ? new ChartSummaryResponse { Current = chartData.Summary.Current, Best = chartData.Summary.Best, Change = chartData.Summary.Change, Trend = chartData.Summary.Trend } : new()
                }
            };
        }).ToList();

        return Ok(chartDtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDashboardChartRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var chartCount = await _dashboardService.GetChartCountAsync(userId);
        if (chartCount >= 20)
            return BadRequest(new { message = "Maximum 20 charts per dashboard" });

        if (RequiresExercise(request.Metric) && !request.ExerciseId.HasValue)
            return BadRequest(new { message = $"Exercise is required for metric '{request.Metric}'" });

        if (request.ExerciseId.HasValue && !await _dashboardService.ExerciseExistsAsync(request.ExerciseId.Value))
            return BadRequest(new { message = "Exercise not found" });

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

        var created = await _dashboardService.CreateAsync(chart);
        var chartData = await _dashboardService.ComputeChartDataAsync(userId, created);

        return CreatedAtAction(nameof(GetAll), new DashboardChartResponse
        {
            Id = created.Id,
            Label = created.Label,
            Metric = created.Metric,
            ExerciseId = created.ExerciseId,
            Period = created.Period,
            ChartType = created.ChartType,
            Position = created.Position,
            Data = new ChartDataResponse
            {
                Points = chartData.Points.Select(p => new ChartDataPointResponse { Date = p.Date, Value = p.Value }).ToList(),
                Summary = new ChartSummaryResponse { Current = chartData.Summary.Current, Best = chartData.Summary.Best, Change = chartData.Summary.Change, Trend = chartData.Summary.Trend }
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDashboardChartRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        if (RequiresExercise(request.Metric) && !request.ExerciseId.HasValue)
            return BadRequest(new { message = $"Exercise is required for metric '{request.Metric}'" });

        var chart = new DashboardChart
        {
            Label = request.Label,
            Metric = request.Metric,
            ExerciseId = request.ExerciseId,
            Period = request.Period,
            ChartType = request.ChartType
        };

        var updated = await _dashboardService.UpdateAsync(id, chart, userId);
        if (updated == null) return NotFound();

        return Ok(new DashboardChartResponse { Id = updated.Id, Label = updated.Label, Metric = updated.Metric, ExerciseId = updated.ExerciseId, Period = updated.Period, ChartType = updated.ChartType, Position = updated.Position });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var deleted = await _dashboardService.DeleteAsync(id, userId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderRequest> items)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var reorderItems = items.Select(i => (i.Id, i.Position)).ToList();
        await _dashboardService.ReorderAsync(reorderItems, userId);
        return Ok();
    }

    private bool RequiresExercise(string metric)
    {
        return metric != "bodyWeight" && metric != "frequency" && metric != "volume";
    }
}
