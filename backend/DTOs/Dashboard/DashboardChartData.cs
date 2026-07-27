namespace GymTracker.DTOs.Dashboard;

public class DashboardChartData
{
    public int ChartId { get; set; }
    public List<ChartDataPoint> Points { get; set; } = new();
    public ChartSummary Summary { get; set; } = new();
}
