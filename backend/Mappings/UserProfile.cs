using AutoMapper;
using GymTracker.DTOs.User;
using GymTracker.Models;

namespace GymTracker.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UpdateProfileRequest, User>();
        CreateMap<UpdateSettingsRequest, UserSettings>();
    }
}
