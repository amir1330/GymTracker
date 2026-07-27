using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services;

public class UserService
{
    private readonly GymDbContext _context;

    public UserService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateSettingsAsync(int userId, string? theme)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = theme ?? "dark"
            };
            _context.UserSettings.Add(settings);
        }
        else
        {
            if (theme != null)
                settings.Theme = theme;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
