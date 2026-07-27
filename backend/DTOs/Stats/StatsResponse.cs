namespace GymTracker.DTOs.Stats;

public class StatsResponse
{
    public int TotalWorkouts { get; set; }
    public int TotalExercises { get; set; }
    public decimal TotalVolume { get; set; }
    public int WorkoutsLast30Days { get; set; }
    public int WorkoutsLast7Days { get; set; }
    public Dictionary<string, int> MuscleGroupCounts { get; set; } = new();
    public List<DailyWorkoutResponse> DailyWorkouts { get; set; } = new();
    public List<ExerciseFrequencyResponse> ExerciseFrequency { get; set; } = new();
}

public class DailyWorkoutResponse
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ExerciseFrequencyResponse
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
