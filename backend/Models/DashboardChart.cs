using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymTracker.Models;

public class DashboardChart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Metric { get; set; } = string.Empty;

    public int? ExerciseId { get; set; }

    [JsonIgnore]
    public Exercise? Exercise { get; set; }

    [Required]
    [MaxLength(10)]
    public string Period { get; set; } = "30d";

    [Required]
    [MaxLength(10)]
    public string ChartType { get; set; } = "line";

    public int Position { get; set; }
}
