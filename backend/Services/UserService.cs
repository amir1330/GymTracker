using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GymTracker.Data;
using GymTracker.DTOs.User;
using GymTracker.Models;

namespace GymTracker.Services;

public class UserService
{
    private readonly GymDbContext _context;
    private readonly IMapper _mapper;

    public UserService(GymDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserSettings?> GetSettingsAsync(int userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<bool> UpdateSettingsAsync(int userId, UpdateSettingsRequest request)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = request.Theme ?? "auto",
                Language = request.Language ?? "en"
            };
            _context.UserSettings.Add(settings);
        }
        else
        {
            _mapper.Map(request, settings);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
