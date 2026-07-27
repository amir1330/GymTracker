using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class WorkoutExercise
{
    public int Id { get; set; }

    public int WorkoutId { get; set; }

    [JsonIgnore]
    public Workout Workout { get; set; } = null!;

    public int ExerciseId { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public int Sets { get; set; }

    public int Reps { get; set; }

    public decimal? Weight { get; set; }

    public int? Duration { get; set; }

    public DurationUnit DurationUnit { get; set; } = DurationUnit.Seconds;

    public int? RestTime { get; set; }
}
