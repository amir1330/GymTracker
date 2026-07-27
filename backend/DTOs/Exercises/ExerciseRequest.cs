using GymTracker.Models;

namespace GymTracker.DTOs.Exercises;

public class ExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public bool IsDuration { get; set; }
    public DurationUnit DurationUnit { get; set; } = DurationUnit.Seconds;
}
