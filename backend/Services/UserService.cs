using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class UserService
{
    private readonly UserManager<User> _userManager;
    private readonly GymDbContext _context;

    public UserService(UserManager<User> userManager, GymDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<UserSettings?> GetSettingsAsync(int userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<bool> UpdateProfileAsync(User user, decimal? weight, decimal? height)
    {
        user.Weight = weight;
        user.Height = height;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateSettingsAsync(int userId, bool? restTimerEnabled, int? defaultRestTimeSeconds, string? theme)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                RestTimerEnabled = restTimerEnabled ?? true,
                DefaultRestTimeSeconds = defaultRestTimeSeconds ?? 90,
                Theme = theme ?? "dark"
            };
            _context.UserSettings.Add(settings);
        }
        else
        {
            if (restTimerEnabled.HasValue)
                settings.RestTimerEnabled = restTimerEnabled.Value;
            if (defaultRestTimeSeconds.HasValue)
                settings.DefaultRestTimeSeconds = defaultRestTimeSeconds.Value;
            if (theme != null)
                settings.Theme = theme;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
