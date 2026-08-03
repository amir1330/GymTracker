namespace GymTracker.Models;

public class UserSettings
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public User User { get; set; } = null!;

    public string Theme { get; set; } = "auto";

    public string Language { get; set; } = "en";
}
