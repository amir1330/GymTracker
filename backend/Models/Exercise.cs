using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class Exercise
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string MuscleGroup { get; set; } = string.Empty;

    public bool IsDuration { get; set; }

    public DurationUnit DurationUnit { get; set; } = DurationUnit.Seconds;

    public bool IsDefault { get; set; }

    [JsonIgnore]
    public ICollection<PresetExercise> PresetExercises { get; set; } = new List<PresetExercise>();

    [JsonIgnore]
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
