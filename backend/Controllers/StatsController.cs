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
public class StatsController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ChartService _chartService;

    public StatsController(GymDbContext context, UserManager<User> userManager, ChartService chartService)
    {
        _context = context;
        _userManager = userManager;
        _chartService = chartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .OrderByDescending(w => w.Date)
            .ToListAsync();

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
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .Take(30)
            .OrderBy(x => x.date)
            .ToList();

        var exerciseFrequency = workouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => we.Exercise.Name)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new { name = g.Key, count = g.Count() })
            .ToList();

        return Ok(new
        {
            totalWorkouts,
            totalExercises,
            totalVolume,
            workoutsLast30Days = last30Days.Count,
            workoutsLast7Days = last7Days.Count,
            muscleGroupCounts,
            dailyWorkouts,
            exerciseFrequency
        });
    }

    [HttpGet("exercise/{exerciseId}/progress")]
    public async Task<IActionResult> GetExerciseProgress(int exerciseId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var exerciseExists = await _context.Exercises
            .AnyAsync(e => e.Id == exerciseId);
        if (!exerciseExists)
        {
            return NotFound(new { message = "Exercise not found" });
        }

        var entries = await _context.WorkoutExercises
            .Where(we => we.ExerciseId == exerciseId && we.Workout.UserId == userId)
            .Include(we => we.Workout)
            .OrderBy(we => we.Workout.Date)
            .Select(we => new
            {
                date = we.Workout.Date.ToString("yyyy-MM-dd"),
                volume = we.Weight.HasValue ? (decimal?)(we.Sets * we.Reps * we.Weight.Value) : null,
                duration = we.Duration,
                restTime = we.RestTime
            })
            .ToListAsync();

        return Ok(new { exerciseId, entries });
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> GetExerciseStats()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var exerciseStats = await _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId)
            .Include(we => we.Exercise)
            .GroupBy(we => new { we.ExerciseId, we.Exercise.Name, we.Exercise.MuscleGroup })
            .Select(g => new
            {
                exerciseId = g.Key.ExerciseId,
                name = g.Key.Name,
                muscleGroup = g.Key.MuscleGroup,
                sessions = g.Count(),
                totalVolume = g.Sum(we => we.Weight.HasValue ? (decimal?)we.Sets * we.Reps * we.Weight.Value : null),
                maxWeight = g.Max(we => we.Weight),
                bestDuration = g.Max(we => we.Duration)
            })
            .ToListAsync();

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
