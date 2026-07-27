namespace GymTracker.DTOs.Dashboard;

public class ChartDataPoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class ChartSummary
{
    public decimal? Current { get; set; }
    public decimal? Best { get; set; }
    public string Change { get; set; } = "0%";
    public string Trend { get; set; } = "flat";
}
