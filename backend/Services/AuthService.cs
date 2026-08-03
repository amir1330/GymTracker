using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data;
using GymTracker.DTOs.Auth;
using GymTracker.Models;

namespace GymTracker.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtService _jwtService;
    private readonly GymDbContext _context;

    public AuthService(UserManager<User> userManager, JwtService jwtService, GymDbContext context)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<(bool Success, string? Token, int? UserId, string? Error)> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return (false, null, null, "Passwords do not match");

        if (request.Password.Length < 6)
            return (false, null, null, "Password must be at least 6 characters");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return (false, null, null, "Email already registered");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(". ", result.Errors.Select(e => e.Description));
            return (false, null, null, errorMessages);
        }

        _context.UserSettings.Add(new UserSettings
        {
            UserId = user.Id,
            Language = request.Language ?? "en"
        });
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.UserName!, user.Email!);
        return (true, token, user.Id, null);
    }

    public async Task<(bool Success, string? Token, int? UserId, string? Error)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return (false, null, null, "Invalid credentials");

        var token = _jwtService.GenerateToken(user.Id, user.UserName!, user.Email!);
        return (true, token, user.Id, null);
    }
}
