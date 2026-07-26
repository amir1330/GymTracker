using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class Preset
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    public ICollection<PresetExercise> PresetExercises { get; set; } = new List<PresetExercise>();
}
