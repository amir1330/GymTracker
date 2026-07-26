using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly GymDbContext _context;

    public ExercisesController(GymDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exercises = await _context.Exercises
            .OrderBy(e => e.MuscleGroup)
            .ThenBy(e => e.Name)
            .ToListAsync();
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        return Ok(exercise);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
    {
        if (await _context.Exercises.AnyAsync(e => e.Name == request.Name))
            return BadRequest(new { message = "Exercise name already exists" });

        var exercise = new Exercise
        {
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            IsDuration = request.IsDuration,
            IsDefault = false
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, exercise);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExerciseRequest request)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        if (exercise.IsDefault) return BadRequest(new { message = "Cannot modify default exercises" });

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.IsDuration = request.IsDuration;

        await _context.SaveChangesAsync();
        return Ok(exercise);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        if (exercise.IsDefault) return BadRequest(new { message = "Cannot delete default exercises" });
        if (await _context.WorkoutExercises.AnyAsync(we => we.ExerciseId == id))
            return BadRequest(new { message = "Exercise is in use" });
        if (await _context.PresetExercises.AnyAsync(pe => pe.ExerciseId == id))
            return BadRequest(new { message = "Exercise is in use" });

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class ExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public bool IsDuration { get; set; }
}
