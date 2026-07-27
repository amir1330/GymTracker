using AutoMapper;
using GymTracker.DTOs.Workouts;
using GymTracker.Models;

namespace GymTracker.Mappings;

public class WorkoutProfile : Profile
{
    public WorkoutProfile()
    {
        CreateMap<CreateWorkoutRequest, Workout>();
        CreateMap<UpdateWorkoutRequest, Workout>();
        CreateMap<WorkoutExerciseRequest, WorkoutExercise>();
    }
}
