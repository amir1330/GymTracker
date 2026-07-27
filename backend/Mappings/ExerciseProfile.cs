using AutoMapper;
using GymTracker.DTOs.Exercises;
using GymTracker.Models;

namespace GymTracker.Mappings;

public class ExerciseProfile : Profile
{
    public ExerciseProfile()
    {
        CreateMap<ExerciseRequest, Exercise>();
    }
}
