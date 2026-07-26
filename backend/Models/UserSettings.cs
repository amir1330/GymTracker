namespace GymTracker.Models;

public class UserSettings
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public bool RestTimerEnabled { get; set; } = true;

    public int DefaultRestTimeSeconds { get; set; } = 90;

    public string Theme { get; set; } = "dark";
}
