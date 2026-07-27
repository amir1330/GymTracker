using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

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

    public async Task<DashboardChart> CreateAsync(DashboardChart chart)
    {
        _context.DashboardCharts.Add(chart);
        await _context.SaveChangesAsync();
        return chart;
    }

    public async Task<DashboardChart?> GetByIdAsync(int id, int userId)
    {
        return await _context.DashboardCharts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
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

        foreach (var chart in charts)
        {
            var workouts = await _chartService.GetWorkoutsForChart(userId, chart.Period, chart.ExerciseId, chart.Metric);
            var points = _chartService.ComputePoints(workouts, chart.Metric);
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
}

public class DashboardChartData
{
    public int ChartId { get; set; }
    public List<ChartDataPoint> Points { get; set; } = new();
    public ChartSummary Summary { get; set; } = new();
}
