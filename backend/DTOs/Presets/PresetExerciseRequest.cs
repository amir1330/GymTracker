namespace GymTracker.DTOs.Presets;

public class PresetExerciseRequest
{
    public int ExerciseId { get; set; }
    public int DefaultSets { get; set; } = 3;
    public int DefaultReps { get; set; } = 10;
    public decimal? DefaultWeight { get; set; }
    public int? DefaultDuration { get; set; }
}
