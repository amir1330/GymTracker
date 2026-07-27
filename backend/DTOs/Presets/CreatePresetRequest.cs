namespace GymTracker.DTOs.Presets;

public class CreatePresetRequest
{
    public string Name { get; set; } = string.Empty;
    public List<PresetExerciseRequest> Exercises { get; set; } = new();
}
