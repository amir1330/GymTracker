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
public class WorkoutsController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<User> _userManager;

    public WorkoutsController(GymDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
        return Ok(workouts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }
        return Ok(workout);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var workout = new Workout
        {
            UserId = userId,
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            Notes = request.Notes,
            BodyWeight = request.BodyWeight,
            WorkoutExercises = request.Exercises.Select(e => new WorkoutExercise
            {
                ExerciseId = e.ExerciseId,
                Sets = e.Sets,
                Reps = e.Reps,
                Weight = e.Weight,
                Duration = e.Duration,
                RestTime = e.RestTime
            }).ToList()
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        await SyncProfileWeight(userId);

        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout);
    }

    [HttpPost("from-preset/{presetId}")]
    public async Task<IActionResult> CreateFromPreset(int presetId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var preset = await _context.Presets
            .Include(p => p.PresetExercises)
            .FirstOrDefaultAsync(p => p.Id == presetId && p.UserId == userId);

        if (preset == null)
        {
            return NotFound(new { message = "Preset not found" });
        }

        var workout = new Workout
        {
            UserId = userId,
            Date = DateTime.UtcNow,
            WorkoutExercises = preset.PresetExercises.Select(pe => new WorkoutExercise
            {
                ExerciseId = pe.ExerciseId,
                Sets = pe.DefaultSets,
                Reps = pe.DefaultReps,
                Weight = pe.DefaultWeight,
                Duration = pe.DefaultDuration
            }).ToList()
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkoutRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .Include(w => w.WorkoutExercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }

        workout.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        workout.Notes = request.Notes;
        workout.BodyWeight = request.BodyWeight;

        _context.WorkoutExercises.RemoveRange(workout.WorkoutExercises);
        workout.WorkoutExercises = request.Exercises.Select(e => new WorkoutExercise
        {
            WorkoutId = id,
            ExerciseId = e.ExerciseId,
            Sets = e.Sets,
            Reps = e.Reps,
            Weight = e.Weight,
            Duration = e.Duration,
            RestTime = e.RestTime
        }).ToList();

        await _context.SaveChangesAsync();

        await SyncProfileWeight(userId);

        return Ok(workout);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{workoutId}/exercises")]
    public async Task<IActionResult> AddExercise(int workoutId, [FromBody] WorkoutExerciseRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }

        var workoutExercise = new WorkoutExercise
        {
            WorkoutId = workoutId,
            ExerciseId = request.ExerciseId,
            Sets = request.Sets,
            Reps = request.Reps,
            Weight = request.Weight,
            Duration = request.Duration,
            RestTime = request.RestTime
        };

        _context.WorkoutExercises.Add(workoutExercise);
        await _context.SaveChangesAsync();

        return Ok(workoutExercise);
    }

    [HttpDelete("{workoutId}/exercises/{workoutExerciseId}")]
    public async Task<IActionResult> RemoveExercise(int workoutId, int workoutExerciseId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }

        var workoutExercise = await _context.WorkoutExercises
            .FirstOrDefaultAsync(we => we.Id == workoutExerciseId && we.WorkoutId == workoutId);

        if (workoutExercise == null)
        {
            return NotFound();
        }

        _context.WorkoutExercises.Remove(workoutExercise);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{workoutId}/exercises/{workoutExerciseId}/restTime")]
    public async Task<IActionResult> UpdateRestTime(int workoutId, int workoutExerciseId, [FromBody] UpdateRestTimeRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

        if (workout == null)
        {
            return NotFound();
        }

        var workoutExercise = await _context.WorkoutExercises
            .FirstOrDefaultAsync(we => we.Id == workoutExerciseId && we.WorkoutId == workoutId);

        if (workoutExercise == null)
        {
            return NotFound();
        }

        workoutExercise.RestTime = request.RestTime;
        await _context.SaveChangesAsync();
        return Ok(workoutExercise);
    }

    private async Task SyncProfileWeight(int userId)
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

public class UpdateRestTimeRequest
{
    public int? RestTime { get; set; }
}

public class CreateWorkoutRequest
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public decimal? BodyWeight { get; set; }
    public List<WorkoutExerciseRequest> Exercises { get; set; } = new();
}

public class UpdateWorkoutRequest
{
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public decimal? BodyWeight { get; set; }
    public List<WorkoutExerciseRequest> Exercises { get; set; } = new();
}

public class WorkoutExerciseRequest
{
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? Duration { get; set; }
    public int? RestTime { get; set; }
}
