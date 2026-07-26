using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class Workout
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public decimal? BodyWeight { get; set; }

    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
