using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresetsController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<User> _userManager;

    public PresetsController(GymDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var presets = await _context.Presets
            .Where(p => p.UserId == userId)
            .Include(p => p.PresetExercises)
                .ThenInclude(pe => pe.Exercise)
            .ToListAsync();
        return Ok(presets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var preset = await _context.Presets
            .Include(p => p.PresetExercises)
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (preset == null)
        {
            return NotFound();
        }
        return Ok(preset);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePresetRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var preset = new Preset
        {
            Name = request.Name,
            UserId = userId,
            PresetExercises = request.Exercises.Select(e => new PresetExercise
            {
                ExerciseId = e.ExerciseId,
                DefaultSets = e.DefaultSets,
                DefaultReps = e.DefaultReps,
                DefaultWeight = e.DefaultWeight,
                DefaultDuration = e.DefaultDuration
            }).ToList()
        };

        _context.Presets.Add(preset);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = preset.Id }, preset);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePresetRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var preset = await _context.Presets
            .Include(p => p.PresetExercises)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (preset == null)
        {
            return NotFound();
        }

        preset.Name = request.Name;

        _context.PresetExercises.RemoveRange(preset.PresetExercises);
        preset.PresetExercises = request.Exercises.Select(e => new PresetExercise
        {
            PresetId = id,
            ExerciseId = e.ExerciseId,
            DefaultSets = e.DefaultSets,
            DefaultReps = e.DefaultReps,
            DefaultWeight = e.DefaultWeight,
            DefaultDuration = e.DefaultDuration
        }).ToList();

        await _context.SaveChangesAsync();
        return Ok(preset);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var preset = await _context.Presets
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (preset == null)
        {
            return NotFound();
        }

        _context.Presets.Remove(preset);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreatePresetRequest
{
    public string Name { get; set; } = string.Empty;
    public List<PresetExerciseRequest> Exercises { get; set; } = new();
}

public class UpdatePresetRequest
{
    public string Name { get; set; } = string.Empty;
    public List<PresetExerciseRequest> Exercises { get; set; } = new();
}

public class PresetExerciseRequest
{
    public int ExerciseId { get; set; }
    public int DefaultSets { get; set; } = 3;
    public int DefaultReps { get; set; } = 10;
    public decimal? DefaultWeight { get; set; }
    public int? DefaultDuration { get; set; }
}
