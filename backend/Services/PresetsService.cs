using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class PresetsService
{
    private readonly GymDbContext _context;

    public PresetsService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<List<Preset>> GetAllAsync(int userId)
    {
        return await _context.Presets
            .Where(p => p.UserId == userId)
            .Include(p => p.PresetExercises)
                .ThenInclude(pe => pe.Exercise)
            .ToListAsync();
    }

    public async Task<Preset?> GetByIdAsync(int id, int userId)
    {
        return await _context.Presets
            .Include(p => p.PresetExercises)
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<Preset> CreateAsync(Preset preset)
    {
        _context.Presets.Add(preset);
        await _context.SaveChangesAsync();
        return preset;
    }

    public async Task<Preset?> UpdateAsync(int id, Preset preset, int userId)
    {
        var existing = await _context.Presets
            .Include(p => p.PresetExercises)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (existing == null) return null;

        existing.Name = preset.Name;

        _context.PresetExercises.RemoveRange(existing.PresetExercises);
        existing.PresetExercises = preset.PresetExercises.ToList();

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var preset = await _context.Presets
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (preset == null) return false;

        _context.Presets.Remove(preset);
        await _context.SaveChangesAsync();
        return true;
    }
}
