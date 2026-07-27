using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymTracker.Models;

namespace GymTracker.Data;

public class GymDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
    {
    }

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Preset> Presets => Set<Preset>();
    public DbSet<PresetExercise> PresetExercises => Set<PresetExercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<DashboardChart> DashboardCharts => Set<DashboardChart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(e => e.UserId);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.UserId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.Exercises)
                .HasForeignKey(e => e.UserId)
                .IsRequired(false);
        });

        modelBuilder.Entity<Preset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Presets)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<PresetExercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Preset)
                .WithMany(p => p.PresetExercises)
                .HasForeignKey(e => e.PresetId);
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.PresetExercises)
                .HasForeignKey(e => e.ExerciseId);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Workouts)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workout)
                .WithMany(w => w.WorkoutExercises)
                .HasForeignKey(e => e.WorkoutId);
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.WorkoutExercises)
                .HasForeignKey(e => e.ExerciseId);
        });

        modelBuilder.Entity<DashboardChart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Exercise)
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .IsRequired(false);
        });
    }
}
