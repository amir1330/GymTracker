using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class StatsService
{
    private readonly GymDbContext _context;

    public StatsService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<List<Workout>> GetWorkoutsAsync(int userId)
    {
        return await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
    }

    public async Task<bool> ExerciseExistsAsync(int exerciseId)
    {
        return await _context.Exercises.AnyAsync(e => e.Id == exerciseId);
    }

    public async Task<List<WorkoutExercise>> GetExerciseProgressAsync(int exerciseId, int userId)
    {
        return await _context.WorkoutExercises
            .Where(we => we.ExerciseId == exerciseId && we.Workout.UserId == userId)
            .Include(we => we.Workout)
            .OrderBy(we => we.Workout.Date)
            .ToListAsync();
    }

    public async Task<List<object>> GetExerciseStatsAsync(int userId)
    {
        return await _context.WorkoutExercises
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
            .ToListAsync<object>();
    }
}
