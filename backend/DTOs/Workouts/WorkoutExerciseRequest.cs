using GymTracker.Models;

namespace GymTracker.DTOs.Workouts;

public class WorkoutExerciseRequest
{
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? Duration { get; set; }
    public DurationUnit DurationUnit { get; set; } = DurationUnit.Seconds;
}
