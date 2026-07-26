using GymTracker.Models;

namespace GymTracker.Data;

public static class SeedData
{
    public static void Initialize(GymDbContext context)
    {
        if (context.Exercises.Any()) return;

        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Bench Press", MuscleGroup = "Chest", IsDefault = true },
            new Exercise { Name = "Incline Bench Press", MuscleGroup = "Chest", IsDefault = true },
            new Exercise { Name = "Dumbbell Press", MuscleGroup = "Chest", IsDefault = true },
            new Exercise { Name = "Cable Fly", MuscleGroup = "Chest", IsDefault = true },
            new Exercise { Name = "Push Up", MuscleGroup = "Chest", IsDefault = true },
            new Exercise { Name = "Deadlift", MuscleGroup = "Back", IsDefault = true },
            new Exercise { Name = "Barbell Row", MuscleGroup = "Back", IsDefault = true },
            new Exercise { Name = "Lat Pulldown", MuscleGroup = "Back", IsDefault = true },
            new Exercise { Name = "Seated Cable Row", MuscleGroup = "Back", IsDefault = true },
            new Exercise { Name = "Pull Up", MuscleGroup = "Back", IsDefault = true },
            new Exercise { Name = "Overhead Press", MuscleGroup = "Shoulders", IsDefault = true },
            new Exercise { Name = "Dumbbell Lateral Raise", MuscleGroup = "Shoulders", IsDefault = true },
            new Exercise { Name = "Face Pull", MuscleGroup = "Shoulders", IsDefault = true },
            new Exercise { Name = "Front Raise", MuscleGroup = "Shoulders", IsDefault = true },
            new Exercise { Name = "Squat", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Leg Press", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Romanian Deadlift", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Leg Curl", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Leg Extension", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Calf Raise", MuscleGroup = "Legs", IsDefault = true },
            new Exercise { Name = "Bicep Curl", MuscleGroup = "Arms", IsDefault = true },
            new Exercise { Name = "Tricep Pushdown", MuscleGroup = "Arms", IsDefault = true },
            new Exercise { Name = "Hammer Curl", MuscleGroup = "Arms", IsDefault = true },
            new Exercise { Name = "Skull Crusher", MuscleGroup = "Arms", IsDefault = true },
            new Exercise { Name = "Plank", MuscleGroup = "Core", IsDuration = true, IsDefault = true },
            new Exercise { Name = "Crunch", MuscleGroup = "Core", IsDefault = true },
            new Exercise { Name = "Hanging Leg Raise", MuscleGroup = "Core", IsDefault = true },
            new Exercise { Name = "Treadmill Run", MuscleGroup = "Cardio", IsDuration = true, IsDefault = true },
            new Exercise { Name = "Cycling", MuscleGroup = "Cardio", IsDuration = true, IsDefault = true },
            new Exercise { Name = "Rowing", MuscleGroup = "Cardio", IsDuration = true, IsDefault = true },
            new Exercise { Name = "Jump Rope", MuscleGroup = "Cardio", IsDuration = true, IsDefault = true }
        };

        context.Exercises.AddRange(exercises);
        context.SaveChanges();
    }
}
