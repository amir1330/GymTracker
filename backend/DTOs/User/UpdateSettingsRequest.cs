namespace GymTracker.DTOs.User;

public class UpdateSettingsRequest
{
    public bool? RestTimerEnabled { get; set; }
    public int? DefaultRestTimeSeconds { get; set; }
    public string? Theme { get; set; }
}
