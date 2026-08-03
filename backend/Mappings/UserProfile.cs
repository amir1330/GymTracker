using AutoMapper;
using GymTracker.DTOs.User;
using GymTracker.Models;

namespace GymTracker.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UpdateSettingsRequest, UserSettings>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
