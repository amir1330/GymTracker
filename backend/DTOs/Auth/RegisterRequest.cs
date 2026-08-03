using System.ComponentModel.DataAnnotations;

namespace GymTracker.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    [RegularExpression(@"^(kz|ru|en)$", ErrorMessage = "Language must be one of: kz, ru, en")]
    public string? Language { get; set; }
}
