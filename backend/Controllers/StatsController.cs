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
    private readonly StatsService _statsService;
    private readonly ChartService _chartService;
    private readonly UserManager<User> _userManager;

    public StatsController(StatsService statsService, ChartService chartService, UserManager<User> userManager)
    {
        _statsService = statsService;
        _chartService = chartService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workouts = await _statsService.GetWorkoutsAsync(userId);

        var totalWorkouts = workouts.Count;
        var totalExercises = workouts.SelectMany(w => w.WorkoutExercises).Count();
        var totalVolume = workouts.SelectMany(w => w.WorkoutExercises)
            .Where(we => we.Weight.HasValue)
            .Sum(we => (decimal)(we.Sets * we.Reps * (we.Weight ?? 0)));

        var last30Days = workouts.Where(w => w.Date >= DateTime.UtcNow.AddDays(-30)).ToList();
        var last7Days = workouts.Where(w => w.Date >= DateTime.UtcNow.AddDays(-7)).ToList();

        var muscleGroupCounts = workouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => we.Exercise.MuscleGroup)
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyWorkouts = workouts
            .GroupBy(w => w.Date.Date)
            .Select(g => new DailyWorkoutResponse { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .Take(30)
            .OrderBy(x => x.Date)
            .ToList();

        var exerciseFrequency = workouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => we.Exercise.Name)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new ExerciseFrequencyResponse { Name = g.Key, Count = g.Count() })
            .ToList();

        return Ok(new StatsResponse
        {
            TotalWorkouts = totalWorkouts,
            TotalExercises = totalExercises,
            TotalVolume = totalVolume,
            WorkoutsLast30Days = last30Days.Count,
            WorkoutsLast7Days = last7Days.Count,
            MuscleGroupCounts = muscleGroupCounts,
            DailyWorkouts = dailyWorkouts,
            ExerciseFrequency = exerciseFrequency
        });
    }

    [HttpGet("exercise/{exerciseId}/progress")]
    public async Task<IActionResult> GetExerciseProgress(int exerciseId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        if (!await _statsService.ExerciseExistsAsync(exerciseId))
            return NotFound(new { message = "Exercise not found" });

        var entries = await _statsService.GetExerciseProgressAsync(exerciseId, userId);
        var response = entries.Select(we => new ExerciseProgressEntry
        {
            Date = we.Workout.Date.ToString("yyyy-MM-dd"),
            Volume = we.Weight.HasValue ? (decimal?)(we.Sets * we.Reps * we.Weight.Value) : null,
            Duration = we.Duration,
            RestTime = we.RestTime
        }).ToList();

        return Ok(new ExerciseProgressResponse { ExerciseId = exerciseId, Entries = response });
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> GetExerciseStats()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var exerciseStats = await _statsService.GetExerciseStatsAsync(userId);
        return Ok(exerciseStats);
    }

    [HttpPost("chart-data")]
    public async Task<IActionResult> GetChartData([FromBody] ChartDataRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var workouts = await _chartService.GetWorkoutsForChart(userId, request.Period, request.ExerciseId);
        var points = _chartService.ComputePoints(workouts, request.Metric);
        var summary = _chartService.ComputeSummary(points);

        return Ok(new { points, summary });
    }
}
