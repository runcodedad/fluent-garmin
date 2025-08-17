using Dynastream.Fit;
using Fluent.Garmin;

namespace Fluent.Garmin.Tests;

public class WorkoutBuilderTests
{
    [Fact]
    public void WorkoutBuilder_ShouldCreateBasicWorkout()
    {
        // Arrange & Act
        var workout = new WorkoutBuilder()
            .Name("Test Workout")
            .Sport(Sport.Running)
            .WarmUp(10, 1)
            .AddTimeStep("Main Set", 20, 4, TargetType.HeartRate)
            .CoolDown(5, 1)
            .Build();

        // Assert
        Assert.Equal("Test Workout", workout.Name);
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);
        
        // Check warm up
        Assert.Equal("Warm Up", workout.Steps[0].Name);
        Assert.Equal(DurationType.Time, workout.Steps[0].Duration?.Type);
        Assert.Equal(600u, workout.Steps[0].Duration?.Value); // 10 minutes * 60 seconds
        Assert.Equal(Intensity.Warmup, workout.Steps[0].Intensity);
        
        // Check main set
        Assert.Equal("Main Set", workout.Steps[1].Name);
        Assert.Equal(DurationType.Time, workout.Steps[1].Duration?.Type);
        Assert.Equal(1200u, workout.Steps[1].Duration?.Value); // 20 minutes * 60 seconds
        Assert.Equal(Intensity.Active, workout.Steps[1].Intensity);
        
        // Check cool down
        Assert.Equal("Cool Down", workout.Steps[2].Name);
        Assert.Equal(Intensity.Cooldown, workout.Steps[2].Intensity);
    }

    [Fact]
    public void WorkoutBuilder_ShouldCreateDistanceSteps()
    {
        // Arrange & Act
        var workout = new WorkoutBuilder()
            .Name("Distance Steps")
            .Sport(Sport.Running)
            .AddDistanceStep("400m Run", 400, 4, TargetType.Speed)
            .Build();

        // Assert
        Assert.Equal("Distance Steps", workout.Name);
        Assert.Single(workout.Steps);
        Assert.Equal("400m Run", workout.Steps[0].Name);
        Assert.Equal(DurationType.Distance, workout.Steps[0].Duration?.Type);
        Assert.Equal(400u, workout.Steps[0].Duration?.Value);
        Assert.Equal(TargetType.Speed, workout.Steps[0].Target?.Type);
        Assert.Equal(4u, workout.Steps[0].Target?.Zone);
    }

    [Fact]
    public void WorkoutBuilder_ShouldSupportFluentChaining()
    {
        // Arrange & Act
        var builder = new WorkoutBuilder();
        var result = builder.Name("Fluent Test")
                           .Sport(Sport.Cycling)
                           .WarmUp(15)
                           .AddTimeStep("Effort", 10, 3, TargetType.Power)
                           .CoolDown(10);

        // Assert - Each method should return the same builder instance for chaining
        Assert.IsType<WorkoutBuilder>(result);
        
        // Build and verify the workout was properly constructed
        var workout = result.Build();
        Assert.Equal("Fluent Test", workout.Name);
        Assert.Equal(Sport.Cycling, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);
    }

    [Fact]
    public void WorkoutBuilder_AddIntervals_ShouldCreateRepeatStructure()
    {
        // Arrange
        var intervalOptions = new IntervalOptions
        {
            DurationType = DurationType.Distance,
            Value = 400,
            Zone = 4,
            TargetType = TargetType.Speed
        };

        var recoveryOptions = new RecoveryOptions
        {
            DurationType = DurationType.Time,
            Value = 120,
            TargetType = TargetType.Open
        };

        // Act
        var workout = new WorkoutBuilder()
            .Name("Interval Test")
            .Sport(Sport.Running)
            .AddIntervals("5x400m", 5, intervalOptions, recoveryOptions)
            .Build();

        // Assert
        Assert.Equal("Interval Test", workout.Name);
        Assert.Single(workout.Steps);

        var repeatStep = workout.Steps[0];
        Assert.Equal("5x400m", repeatStep.Name);
        Assert.True(repeatStep.IsRepeat);
        Assert.Equal(5u, repeatStep.RepeatCount);
        Assert.Equal(DurationType.RepeatUntilStepsCmplt, repeatStep.Duration?.Type);
        Assert.Equal(5u, repeatStep.Duration?.Value);
        Assert.Equal(2, repeatStep.RepeatSteps.Count);

        // Check interval step
        var intervalStep = repeatStep.RepeatSteps[0];
        Assert.Equal("Interval", intervalStep.Name);
        Assert.Equal(DurationType.Distance, intervalStep.Duration?.Type);
        Assert.Equal(400u, intervalStep.Duration?.Value);
        Assert.Equal(TargetType.Speed, intervalStep.Target?.Type);
        Assert.Equal(4u, intervalStep.Target?.Zone);

        // Check recovery step
        var recoveryStep = repeatStep.RepeatSteps[1];
        Assert.Equal("Recovery", recoveryStep.Name);
        Assert.Equal(DurationType.Time, recoveryStep.Duration?.Type);
        Assert.Equal(120u, recoveryStep.Duration?.Value);
        Assert.Equal(Intensity.Rest, recoveryStep.Intensity);
        Assert.Equal(TargetType.Open, recoveryStep.Target?.Type);
    }

    [Fact]
    public void WorkoutBuilder_AddRepeat_ShouldCreateCustomRepeatStructure()
    {
        // Arrange
        var buildStep = new WorkoutStep
        {
            Name = "Build",
            Duration = new StepDuration { Type = DurationType.Time, Value = 300 }, // 5 min
            Intensity = Intensity.Active,
            Target = new StepTarget { Type = TargetType.Power, Zone = 3 }
        };

        var restStep = new WorkoutStep
        {
            Name = "Rest",
            Duration = new StepDuration { Type = DurationType.Time, Value = 180 }, // 3 min
            Intensity = Intensity.Rest,
            Target = new StepTarget { Type = TargetType.Open }
        };

        // Act
        var workout = new WorkoutBuilder()
            .Name("Custom Repeat Test")
            .Sport(Sport.Cycling)
            .AddRepeat("3x Build/Rest", 3, buildStep, restStep)
            .Build();

        // Assert
        Assert.Equal("Custom Repeat Test", workout.Name);
        Assert.Single(workout.Steps);

        var repeatStep = workout.Steps[0];
        Assert.Equal("3x Build/Rest", repeatStep.Name);
        Assert.True(repeatStep.IsRepeat);
        Assert.Equal(3u, repeatStep.RepeatCount);
        Assert.Equal(DurationType.RepeatUntilStepsCmplt, repeatStep.Duration?.Type);
        Assert.Equal(2, repeatStep.RepeatSteps.Count);

        // Check that the steps are properly copied
        Assert.Equal("Build", repeatStep.RepeatSteps[0].Name);
        Assert.Equal("Rest", repeatStep.RepeatSteps[1].Name);
    }

    [Fact]
    public void IWorkoutBuilder_ShouldSupportDependencyInjection()
    {
        // Arrange - Demonstrate interface can be used for DI
        IWorkoutBuilder builder = new WorkoutBuilder();

        // Act
        var workout = builder
            .Name("DI Test Workout")
            .Sport(Sport.Running)
            .WarmUp(5, 1)
            .AddTimeStep("Easy Run", 30, 2)
            .CoolDown(5, 1)
            .Build();

        // Assert
        Assert.Equal("DI Test Workout", workout.Name);
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);
        Assert.Equal("Warm Up", workout.Steps[0].Name);
        Assert.Equal("Easy Run", workout.Steps[1].Name);
        Assert.Equal("Cool Down", workout.Steps[2].Name);
    }
}