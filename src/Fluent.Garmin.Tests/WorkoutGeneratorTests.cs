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
        Assert.Equal(3, workout.Steps.Count);

        // Check warm up
        var warmUp = workout.Steps[0];
        Assert.Equal("Warm Up", warmUp.Name);
        Assert.Equal(DurationType.Time, warmUp.Duration?.Type);
        Assert.Equal(600u, warmUp.Duration?.Value); // 10 minutes
        Assert.Equal(Intensity.Warmup, warmUp.Intensity);
        Assert.Equal(TargetType.HeartRate, warmUp.Target?.Type);
        Assert.Equal(1u, warmUp.Target?.Zone);

        // Check repeat step
        var repeatStep = workout.Steps[1];
        Assert.Equal("5x400m", repeatStep.Name);
        Assert.True(repeatStep.IsRepeat);
        Assert.Equal(5u, repeatStep.RepeatCount);
        Assert.Equal(DurationType.RepeatUntilStepsCmplt, repeatStep.Duration?.Type);
        Assert.Equal(2, repeatStep.RepeatSteps.Count);

        // Check interval step
        var intervalStep = repeatStep.RepeatSteps[0];
        Assert.Equal("Interval", intervalStep.Name);
        Assert.Equal(DurationType.Distance, intervalStep.Duration?.Type);
        Assert.Equal(400u, intervalStep.Duration?.Value);
        Assert.Equal(Intensity.Active, intervalStep.Intensity);
        Assert.Equal(TargetType.Speed, intervalStep.Target?.Type);
        Assert.Equal(4u, intervalStep.Target?.Zone);

        // Check recovery step
        var recoveryStep = repeatStep.RepeatSteps[1];
        Assert.Equal("Recovery", recoveryStep.Name);
        Assert.Equal(DurationType.Time, recoveryStep.Duration?.Type);
        Assert.Equal(120u, recoveryStep.Duration?.Value);
        Assert.Equal(Intensity.Rest, recoveryStep.Intensity);
        Assert.Equal(TargetType.Open, recoveryStep.Target?.Type);

        // Check cool down
        var coolDown = workout.Steps[2];
        Assert.Equal("Cool Down", coolDown.Name);
        Assert.Equal(DurationType.Time, coolDown.Duration?.Type);
        Assert.Equal(600u, coolDown.Duration?.Value); // 10 minutes
        Assert.Equal(Intensity.Cooldown, coolDown.Intensity);
        Assert.Equal(TargetType.HeartRate, coolDown.Target?.Type);
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
        Assert.Equal(3, workout.Steps.Count);

        // Check warm up
        var warmUp = workout.Steps[0];
        Assert.Equal("Warm Up", warmUp.Name);
        Assert.Equal(900u, warmUp.Duration?.Value); // 15 minutes
        Assert.Equal(Intensity.Warmup, warmUp.Intensity);
        Assert.Equal(TargetType.Power, warmUp.Target?.Type);
        Assert.Equal(1u, warmUp.Target?.Zone);

        // Check repeat step
        var repeatStep = workout.Steps[1];
        Assert.Equal("3x Build/Recovery", repeatStep.Name);
        Assert.True(repeatStep.IsRepeat);
        Assert.Equal(3u, repeatStep.RepeatCount);
        Assert.Equal(DurationType.RepeatUntilStepsCmplt, repeatStep.Duration?.Type);
        Assert.Equal(2, repeatStep.RepeatSteps.Count);

        // Check build step
        var buildStep = repeatStep.RepeatSteps[0];
        Assert.Equal("Build", buildStep.Name);
        Assert.Equal(300u, buildStep.Duration?.Value); // 5 minutes
        Assert.Equal(Intensity.Active, buildStep.Intensity);
        Assert.Equal(TargetType.Power, buildStep.Target?.Type);
        Assert.Equal(3u, buildStep.Target?.Zone);

        // Check recovery step
        var recoveryStep = repeatStep.RepeatSteps[1];
        Assert.Equal("Recovery", recoveryStep.Name);
        Assert.Equal(180u, recoveryStep.Duration?.Value); // 3 minutes
        Assert.Equal(Intensity.Rest, recoveryStep.Intensity);
        Assert.Equal(TargetType.Power, recoveryStep.Target?.Type);
        Assert.Equal(1u, recoveryStep.Target?.Zone);

        // Check cool down
        var coolDown = workout.Steps[2];
        Assert.Equal("Cool Down", coolDown.Name);
        Assert.Equal(600u, coolDown.Duration?.Value); // 10 minutes
        Assert.Equal(Intensity.Cooldown, coolDown.Intensity);
        Assert.Equal(TargetType.Power, coolDown.Target?.Type);
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

    [Fact]
    public void GenerateWorkoutFile_ShouldHandleCustomTargetRanges()
    {
        // Arrange
        var workout = new WorkoutModel
        {
            Name = "Custom Range Test",
            Sport = Sport.Running,
            Steps = new List<WorkoutStep>
            {
                new WorkoutStep
                {
                    Name = "Heart Rate Range",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 600 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget 
                    { 
                        Type = TargetType.HeartRate, 
                        LowValue = 150, 
                        HighValue = 170 
                    }
                },
                new WorkoutStep
                {
                    Name = "Power Range",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 300 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget 
                    { 
                        Type = TargetType.Power, 
                        LowValue = 200, 
                        HighValue = 250 
                    }
                },
                new WorkoutStep
                {
                    Name = "Speed Range",
                    Duration = new StepDuration { Type = DurationType.Distance, Value = 1000 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget 
                    { 
                        Type = TargetType.Speed, 
                        LowValue = 12, 
                        HighValue = 15 
                    }
                },
                new WorkoutStep
                {
                    Name = "Cadence Range",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 300 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget 
                    { 
                        Type = TargetType.Cadence, 
                        LowValue = 85, 
                        HighValue = 95 
                    }
                }
            }
        };
        var tempFile = Path.GetTempFileName() + ".fit";

        try
        {
            // Act - This should not throw an exception and should handle custom ranges
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