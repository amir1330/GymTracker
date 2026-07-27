using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class ExercisesService
{
    private readonly GymDbContext _context;

    public ExercisesService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _context.Exercises
            .OrderBy(e => e.MuscleGroup)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _context.Exercises.FindAsync(id);
    }

    public async Task<Exercise> CreateAsync(Exercise exercise)
    {
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return exercise;
    }

    public async Task<Exercise?> UpdateAsync(int id, Exercise exercise)
    {
        var existing = await _context.Exercises.FindAsync(id);
        if (existing == null) return null;
        if (existing.IsDefault) return null;

        existing.Name = exercise.Name;
        existing.MuscleGroup = exercise.MuscleGroup;
        existing.IsDuration = exercise.IsDuration;
        existing.DurationUnit = exercise.DurationUnit;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return false;
        if (exercise.IsDefault) return false;
        if (await _context.WorkoutExercises.AnyAsync(we => we.ExerciseId == id)) return false;
        if (await _context.PresetExercises.AnyAsync(pe => pe.ExerciseId == id)) return false;

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Exercises.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Exercises.AnyAsync(e => e.Name == name);
    }

    public async Task<bool> IsDefaultAsync(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        return exercise?.IsDefault ?? false;
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        return await _context.WorkoutExercises.AnyAsync(we => we.ExerciseId == id) ||
               await _context.PresetExercises.AnyAsync(pe => pe.ExerciseId == id);
    }
}
