namespace GymTracker.DTOs.Stats;

public class ExerciseProgressResponse
{
    public int ExerciseId { get; set; }
    public List<ExerciseProgressEntry> Entries { get; set; } = new();
}

public class ExerciseProgressEntry
{
    public string Date { get; set; } = string.Empty;
    public decimal? Volume { get; set; }
    public int? Duration { get; set; }
    public int? RestTime { get; set; }
}
