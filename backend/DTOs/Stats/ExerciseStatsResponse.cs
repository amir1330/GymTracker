namespace GymTracker.DTOs.Stats;

public class ExerciseStatsResponse
{
    public int ExerciseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public int Sessions { get; set; }
    public decimal? TotalVolume { get; set; }
    public decimal? MaxWeight { get; set; }
    public int? BestDuration { get; set; }
}
