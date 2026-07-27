namespace GymTracker.DTOs.Dashboard;

public class DashboardChartResponse
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public int? ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public string Period { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public int Position { get; set; }
    public ChartDataResponse Data { get; set; } = new();
}

public class ChartDataResponse
{
    public List<ChartDataPointResponse> Points { get; set; } = new();
    public ChartSummaryResponse Summary { get; set; } = new();
}

public class ChartDataPointResponse
{
    public string Date { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class ChartSummaryResponse
{
    public decimal? Current { get; set; }
    public decimal? Best { get; set; }
    public string Change { get; set; } = "0%";
    public string Trend { get; set; } = "flat";
}
