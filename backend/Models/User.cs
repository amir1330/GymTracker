using Microsoft.AspNetCore.Identity;

namespace GymTracker.Models;

public class User : IdentityUser<int>
{
    public decimal? Weight { get; set; }

    public decimal? Height { get; set; }

    public UserSettings? Settings { get; set; }

    public ICollection<Preset> Presets { get; set; } = new List<Preset>();

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
