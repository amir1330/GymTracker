using AutoMapper;
using GymTracker.DTOs.Presets;
using GymTracker.Models;

namespace GymTracker.Mappings;

public class PresetProfile : Profile
{
    public PresetProfile()
    {
        CreateMap<CreatePresetRequest, Preset>();
        CreateMap<UpdatePresetRequest, Preset>();
        CreateMap<PresetExerciseRequest, PresetExercise>();
    }
}
