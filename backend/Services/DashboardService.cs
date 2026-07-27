using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;
using GymTracker.DTOs.Dashboard;

namespace GymTracker.Services;

public class DashboardService
{
    private readonly GymDbContext _context;
    private readonly ChartService _chartService;

    public DashboardService(GymDbContext context, ChartService chartService)
    {
        _context = context;
        _chartService = chartService;
    }

    public async Task<List<DashboardChart>> GetAllAsync(int userId)
    {
        return await _context.DashboardCharts
            .Where(c => c.UserId == userId)
            .Include(c => c.Exercise)
            .OrderBy(c => c.Position)
            .ToListAsync();
    }

    public async Task<int> GetChartCountAsync(int userId)
    {
        return await _context.DashboardCharts.CountAsync(c => c.UserId == userId);
    }

    public async Task<bool> ExerciseExistsAsync(int exerciseId)
    {
        return await _context.Exercises.AnyAsync(e => e.Id == exerciseId);
    }

    public async Task<string?> GetExerciseNameAsync(int exerciseId)
    {
        var exercise = await _context.Exercises.FindAsync(exerciseId);
        return exercise?.Name;
    }

    public async Task<DashboardChart> CreateAsync(DashboardChart chart)
    {
        _context.DashboardCharts.Add(chart);
        await _context.SaveChangesAsync();
        return chart;
    }

    public async Task<DashboardChart?> UpdateAsync(int id, DashboardChart chart, int userId)
    {
        var existing = await _context.DashboardCharts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (existing == null) return null;

        existing.Label = chart.Label;
        existing.Metric = chart.Metric;
        existing.ExerciseId = chart.ExerciseId;
        existing.Period = chart.Period;
        existing.ChartType = chart.ChartType;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var chart = await _context.DashboardCharts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (chart == null) return false;

        _context.DashboardCharts.Remove(chart);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ReorderAsync(List<(int Id, int Position)> items, int userId)
    {
        var ids = items.Select(i => i.Id).ToList();
        var charts = await _context.DashboardCharts
            .Where(c => ids.Contains(c.Id) && c.UserId == userId)
            .ToListAsync();

        foreach (var chart in charts)
        {
            var item = items.FirstOrDefault(i => i.Id == chart.Id);
            if (item.Id != 0)
            {
                chart.Position = item.Position;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<DashboardChartData>> GetAllChartDataAsync(int userId, List<DashboardChart> charts)
    {
        var result = new List<DashboardChartData>();

        var allExerciseIds = charts
            .Where(c => c.ExerciseId.HasValue)
            .Select(c => c.ExerciseId!.Value)
            .Distinct()
            .ToList();

        var allMetrics = charts.Select(c => c.Metric).Distinct().ToList();
        var needsWorkoutExercises = allMetrics.Any(m => m != "bodyWeight" && m != "frequency");
        var needsBodyWeight = allMetrics.Contains("bodyWeight");

        var minPeriod = charts.Min(c => GetCutoffDate(c.Period));

        List<WorkoutExercise> allWorkoutExercises = new();
        if (needsWorkoutExercises)
        {
            allWorkoutExercises = await _chartService.GetWorkoutsForChartBatch(userId, minPeriod, allExerciseIds);
        }

        foreach (var chart in charts)
        {
            List<WorkoutExercise> chartWorkouts;
            if (chart.Metric == "bodyWeight" || chart.Metric == "frequency")
            {
                chartWorkouts = await _chartService.GetWorkoutsForChart(userId, chart.Period, chart.ExerciseId, chart.Metric);
            }
            else
            {
                var cutoff = GetCutoffDate(chart.Period);
                chartWorkouts = allWorkoutExercises
                    .Where(we => we.Workout.Date >= cutoff && (!chart.ExerciseId.HasValue || we.ExerciseId == chart.ExerciseId.Value))
                    .ToList();
            }

            var points = _chartService.ComputePoints(chartWorkouts, chart.Metric);
            var summary = _chartService.ComputeSummary(points);

            result.Add(new DashboardChartData
            {
                ChartId = chart.Id,
                Points = points,
                Summary = summary
            });
        }

        return result;
    }

    public async Task<DashboardChartData> ComputeChartDataAsync(int userId, DashboardChart chart)
    {
        var workouts = await _chartService.GetWorkoutsForChart(userId, chart.Period, chart.ExerciseId, chart.Metric);
        var points = _chartService.ComputePoints(workouts, chart.Metric);
        var summary = _chartService.ComputeSummary(points);

        return new DashboardChartData
        {
            ChartId = chart.Id,
            Points = points,
            Summary = summary
        };
    }

    private static DateTime GetCutoffDate(string period)
    {
        return period switch
        {
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            "90d" => DateTime.UtcNow.AddDays(-90),
            "180d" => DateTime.UtcNow.AddDays(-180),
            "365d" => DateTime.UtcNow.AddDays(-365),
            "all" => DateTime.MinValue,
            _ => DateTime.UtcNow.AddDays(-30)
        };
    }
}
