using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Workouts;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly WorkoutsService _workoutsService;
    private readonly PresetsService _presetsService;
    private readonly UserManager<User> _userManager;

    public WorkoutsController(WorkoutsService workoutsService, PresetsService presetsService, UserManager<User> userManager)
    {
        _workoutsService = workoutsService;
        _presetsService = presetsService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workouts = await _workoutsService.GetAllAsync(userId);
        return Ok(workouts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _workoutsService.GetByIdAsync(id, userId);
        if (workout == null) return NotFound();
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
                DurationUnit = e.DurationUnit,
                RestTime = e.RestTime
            }).ToList()
        };

        var created = await _workoutsService.CreateAsync(workout);
        await _workoutsService.SyncProfileWeightAsync(userId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("from-preset/{presetId}")]
    public async Task<IActionResult> CreateFromPreset(int presetId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var preset = await _presetsService.GetByIdAsync(presetId, userId);
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
                Duration = pe.DefaultDuration,
                DurationUnit = pe.Exercise.DurationUnit
            }).ToList()
        };

        var created = await _workoutsService.CreateAsync(workout);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkoutRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var workout = new Workout
        {
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            Notes = request.Notes,
            BodyWeight = request.BodyWeight,
            WorkoutExercises = request.Exercises.Select(e => new WorkoutExercise
            {
                WorkoutId = id,
                ExerciseId = e.ExerciseId,
                Sets = e.Sets,
                Reps = e.Reps,
                Weight = e.Weight,
                Duration = e.Duration,
                DurationUnit = e.DurationUnit,
                RestTime = e.RestTime
            }).ToList()
        };

        var updated = await _workoutsService.UpdateAsync(id, workout, userId);
        if (updated == null) return NotFound();

        await _workoutsService.SyncProfileWeightAsync(userId);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var deleted = await _workoutsService.DeleteAsync(id, userId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{workoutId}/exercises")]
    public async Task<IActionResult> AddExercise(int workoutId, [FromBody] WorkoutExerciseRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _workoutsService.GetByIdAsync(workoutId, userId);
        if (workout == null) return NotFound();

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

        return Ok(workoutExercise);
    }

    [HttpDelete("{workoutId}/exercises/{workoutExerciseId}")]
    public async Task<IActionResult> RemoveExercise(int workoutId, int workoutExerciseId)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _workoutsService.GetByIdAsync(workoutId, userId);
        if (workout == null) return NotFound();

        return NoContent();
    }

    [HttpPut("{workoutId}/exercises/{workoutExerciseId}/restTime")]
    public async Task<IActionResult> UpdateRestTime(int workoutId, int workoutExerciseId, [FromBody] UpdateRestTimeRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var workout = await _workoutsService.GetByIdAsync(workoutId, userId);
        if (workout == null) return NotFound();

        return Ok(new { message = "Rest time updated" });
    }
}
