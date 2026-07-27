namespace GymTracker.DTOs.Dashboard;

public class DashboardChartRequest
{
    public string Label { get; set; } = string.Empty;
    public string Metric { get; set; } = "weight";
    public int? ExerciseId { get; set; }
    public string Period { get; set; } = "30d";
    public string ChartType { get; set; } = "line";
}
