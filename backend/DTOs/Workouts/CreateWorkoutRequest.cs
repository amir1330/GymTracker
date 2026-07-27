namespace GymTracker.DTOs.Workouts;

public class CreateWorkoutRequest
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public decimal? BodyWeight { get; set; }
    public List<WorkoutExerciseRequest> Exercises { get; set; } = new();
}
