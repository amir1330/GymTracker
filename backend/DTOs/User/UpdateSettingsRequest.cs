using System.ComponentModel.DataAnnotations;

namespace GymTracker.DTOs.User;

public class UpdateSettingsRequest
{
    public string? Theme { get; set; }

    [RegularExpression(@"^(kz|ru|en)$", ErrorMessage = "Language must be one of: kz, ru, en")]
    public string? Language { get; set; }
}
