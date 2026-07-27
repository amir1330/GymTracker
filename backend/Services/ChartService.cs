using System.Globalization;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;
using GymTracker.DTOs.Dashboard;

namespace GymTracker.Services;

public class ChartService
{
    private readonly GymDbContext _context;

    public ChartService(GymDbContext context)
    {
        _context = context;
    }

    public DateTime GetCutoffDate(string period)
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

    public List<ChartDataPoint> ComputePoints(List<WorkoutExercise> workouts, string metric)
    {
        var grouped = workouts.GroupBy(we => we.Workout.Date.Date).OrderBy(g => g.Key);

        return metric switch
        {
            "weight" => grouped.Select(g => new ChartDataPoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Value = g.Max(we => we.Weight ?? 0)
            }).ToList(),

            "volume" => grouped.Select(g => new ChartDataPoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Value = g.Sum(we => we.Weight.HasValue ? (decimal)(we.Sets * we.Reps * we.Weight.Value) : 0)
            }).ToList(),

            "duration" => grouped.Select(g => new ChartDataPoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Value = g.Where(we => we.Duration.HasValue).Sum(we => NormalizeToSeconds(we.Duration!.Value, we.DurationUnit))
            }).ToList(),

            "bodyWeight" => grouped
                .Where(g => g.Any(we => we.Workout.BodyWeight.HasValue))
                .Select(g => new ChartDataPoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.First(we => we.Workout.BodyWeight.HasValue).Workout.BodyWeight ?? 0
                }).ToList(),

            "frequency" => workouts
                .GroupBy(we => ISOWeek.GetWeekOfYear(we.Workout.Date))
                .OrderBy(g => g.First().Workout.Date)
                .Select(g => new ChartDataPoint
                {
                    Date = g.First().Workout.Date.ToString("yyyy-MM-dd"),
                    Value = g.Select(we => we.WorkoutId).Distinct().Count()
                }).ToList(),

            _ => new List<ChartDataPoint>()
        };
    }

    public ChartSummary ComputeSummary(List<ChartDataPoint> points)
    {
        if (points.Count == 0)
        {
            return new ChartSummary
            {
                Current = null,
                Best = null,
                Change = "0%",
                Trend = "flat"
            };
        }

        var current = points.Last().Value;
        var best = points.Max(p => p.Value);
        var first = points.First().Value;
        var changePercent = first != 0 ? ((current - first) / Math.Abs(first)) * 100 : 0;
        var trend = changePercent > 1 ? "up" : changePercent < -1 ? "down" : "flat";

        return new ChartSummary
        {
            Current = current,
            Best = best,
            Change = $"{changePercent:+0.0;-0.0}%",
            Trend = trend
        };
    }

    public async Task<List<WorkoutExercise>> GetWorkoutsForChart(int userId, string period, int? exerciseId, string metric)
    {
        var cutoff = GetCutoffDate(period);

        if (metric == "bodyWeight")
        {
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId && w.Date >= cutoff && w.BodyWeight.HasValue)
                .OrderBy(w => w.Date)
                .ToListAsync();

            return workouts.Select(w => new WorkoutExercise
            {
                WorkoutId = w.Id,
                Workout = w
            }).ToList();
        }

        var query = _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId && we.Workout.Date >= cutoff)
            .Include(we => we.Workout)
            .Include(we => we.Exercise)
            .AsQueryable();

        if (exerciseId.HasValue)
        {
            query = query.Where(we => we.ExerciseId == exerciseId.Value);
        }

        return await query.OrderBy(we => we.Workout.Date).ToListAsync();
    }

    public async Task<List<WorkoutExercise>> GetWorkoutsForChartBatch(int userId, DateTime minCutoff, List<int> exerciseIds)
    {
        var query = _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId && we.Workout.Date >= minCutoff)
            .Include(we => we.Workout)
            .Include(we => we.Exercise)
            .AsQueryable();

        if (exerciseIds.Any())
        {
            query = query.Where(we => exerciseIds.Contains(we.ExerciseId));
        }

        return await query.OrderBy(we => we.Workout.Date).ToListAsync();
    }

    private static decimal NormalizeToSeconds(int value, DurationUnit unit)
    {
        return unit switch
        {
            DurationUnit.Seconds => value,
            DurationUnit.Minutes => value * 60,
            DurationUnit.Hours => value * 3600,
            _ => value
        };
    }
}
