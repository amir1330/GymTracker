using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class WorkoutsService
{
    private readonly GymDbContext _context;
    private readonly UserManager<User> _userManager;

    public WorkoutsService(GymDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<Workout>> GetAllAsync(int userId)
    {
        return await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
    }

    public async Task<Workout?> GetByIdAsync(int id, int userId)
    {
        return await _context.Workouts
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
    }

    public async Task<Workout> CreateAsync(Workout workout)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();
        return workout;
    }

    public async Task<Workout?> UpdateAsync(int id, Workout workout, int userId)
    {
        var existing = await _context.Workouts
            .Include(w => w.WorkoutExercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (existing == null) return null;

        existing.Date = workout.Date;
        existing.Notes = workout.Notes;
        existing.BodyWeight = workout.BodyWeight;

        _context.WorkoutExercises.RemoveRange(existing.WorkoutExercises);
        existing.WorkoutExercises = workout.WorkoutExercises.ToList();

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null) return false;

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SyncProfileWeightAsync(int userId)
    {
        var latestWeight = await _context.Workouts
            .Where(w => w.UserId == userId && w.BodyWeight.HasValue)
            .OrderByDescending(w => w.Date)
            .Select(w => w.BodyWeight)
            .FirstOrDefaultAsync();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.Weight = latestWeight;
            await _userManager.UpdateAsync(user);
        }
    }
}
