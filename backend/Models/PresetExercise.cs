using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class PresetExercise
{
    public int Id { get; set; }

    public int PresetId { get; set; }

    [JsonIgnore]
    public Preset Preset { get; set; } = null!;

    public int ExerciseId { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public int DefaultSets { get; set; } = 3;

    public int DefaultReps { get; set; } = 10;

    public decimal? DefaultWeight { get; set; }

    public int? DefaultDuration { get; set; }
}
