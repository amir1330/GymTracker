namespace GymTracker.DTOs.Workouts;

public class UpdateWorkoutRequest
{
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public decimal? BodyWeight { get; set; }
    public List<WorkoutExerciseRequest> Exercises { get; set; } = new();
}
