using Dynastream.Fit;
using Fluent.Garmin;

namespace Fluent.Garmin.Tests;

public class WorkoutGeneratorTests
{
    [Fact]
    public void CreateIntervalWorkoutModel_ShouldCreateValidWorkout()
    {
        // Act
        var workout = WorkoutGenerator.CreateIntervalWorkoutModel();

        // Assert
        Assert.Equal("5x400m Track Intervals", workout.Name);
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.Equal(6, workout.Steps.Count);

        // Check warm up
        var warmUp = workout.Steps[0];
        Assert.Equal("Warm Up", warmUp.Name);
        Assert.Equal(DurationType.Time, warmUp.Duration?.Type);
        Assert.Equal(600u, warmUp.Duration?.Value); // 10 minutes
        Assert.Equal(Intensity.Warmup, warmUp.Intensity);
        Assert.Equal(TargetType.HeartRate, warmUp.Target?.Type);
        Assert.Equal(1u, warmUp.Target?.Zone);

        // Check first interval
        var interval1 = workout.Steps[1];
        Assert.Equal("Interval 1", interval1.Name);
        Assert.Equal(DurationType.Distance, interval1.Duration?.Type);
        Assert.Equal(400u, interval1.Duration?.Value);
        Assert.Equal(Intensity.Active, interval1.Intensity);
        Assert.Equal(TargetType.Speed, interval1.Target?.Type);
        Assert.Equal(4u, interval1.Target?.Zone);

        // Check first recovery
        var recovery1 = workout.Steps[2];
        Assert.Equal("Recovery 1", recovery1.Name);
        Assert.Equal(DurationType.Time, recovery1.Duration?.Type);
        Assert.Equal(120u, recovery1.Duration?.Value); // 2 minutes
        Assert.Equal(Intensity.Rest, recovery1.Intensity);
        Assert.Equal(TargetType.Open, recovery1.Target?.Type);

        // Check cool down
        var coolDown = workout.Steps[5];
        Assert.Equal("Cool Down", coolDown.Name);
        Assert.Equal(Intensity.Cooldown, coolDown.Intensity);
    }

    [Fact]
    public void CreateTempoRunModel_ShouldCreateValidWorkout()
    {
        // Act
        var workout = WorkoutGenerator.CreateTempoRunModel();

        // Assert
        Assert.Equal("20 Minute Tempo Run", workout.Name);
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);

        // Check warm up
        var warmUp = workout.Steps[0];
        Assert.Equal("Warm Up", warmUp.Name);
        Assert.Equal(900u, warmUp.Duration?.Value); // 15 minutes
        Assert.Equal(Intensity.Warmup, warmUp.Intensity);
        Assert.Equal(2u, warmUp.Target?.Zone);

        // Check tempo portion
        var tempo = workout.Steps[1];
        Assert.Equal("Tempo", tempo.Name);
        Assert.Equal(1200u, tempo.Duration?.Value); // 20 minutes
        Assert.Equal(Intensity.Active, tempo.Intensity);
        Assert.Equal(4u, tempo.Target?.Zone);

        // Check cool down
        var coolDown = workout.Steps[2];
        Assert.Equal("Cool Down", coolDown.Name);
        Assert.Equal(600u, coolDown.Duration?.Value); // 10 minutes
        Assert.Equal(Intensity.Cooldown, coolDown.Intensity);
    }

    [Fact]
    public void CreateCyclingWorkoutModel_ShouldCreateValidWorkout()
    {
        // Act
        var workout = WorkoutGenerator.CreateCyclingWorkoutModel();

        // Assert
        Assert.Equal("Power Intervals", workout.Name);
        Assert.Equal(Sport.Cycling, workout.Sport);
        Assert.Equal(5, workout.Steps.Count);

        // Check that all steps have power targets
        foreach (var step in workout.Steps)
        {
            Assert.Equal(TargetType.Power, step.Target?.Type);
        }

        // Check build intervals
        var build1 = workout.Steps[1];
        Assert.Equal("Build 1", build1.Name);
        Assert.Equal(300u, build1.Duration?.Value); // 5 minutes
        Assert.Equal(3u, build1.Target?.Zone);

        var build2 = workout.Steps[3];
        Assert.Equal("Build 2", build2.Name);
        Assert.Equal(300u, build2.Duration?.Value); // 5 minutes
        Assert.Equal(4u, build2.Target?.Zone);
    }

    [Fact]
    public void GenerateWorkoutFile_ShouldThrowExceptionForNullWorkout()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WorkoutGenerator.GenerateWorkoutFile(null!, "test.fit"));
    }

    [Fact]
    public void GenerateWorkoutFile_ShouldThrowExceptionForEmptySteps()
    {
        // Arrange
        var workout = new WorkoutModel { Name = "Empty", Steps = new List<WorkoutStep>() };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WorkoutGenerator.GenerateWorkoutFile(workout, "test.fit"));
    }

    [Fact]
    public void GenerateWorkoutFile_ShouldHandleValidWorkout()
    {
        // Arrange
        var workout = WorkoutGenerator.CreateIntervalWorkoutModel();
        var tempFile = Path.GetTempFileName() + ".fit";

        try
        {
            // Act - This should not throw an exception
            WorkoutGenerator.GenerateWorkoutFile(workout, tempFile);

            // Assert
            Assert.True(System.IO.File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            // Cleanup
            if (System.IO.File.Exists(tempFile))
                System.IO.File.Delete(tempFile);
        }
    }
}