namespace GymTracker.DTOs.Stats;

public class ChartDataRequest
{
    public string Metric { get; set; } = "weight";
    public int? ExerciseId { get; set; }
    public string Period { get; set; } = "30d";
}
