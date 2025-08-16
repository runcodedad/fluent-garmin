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
            .TimeInterval("Main Set", 20, 4, TargetType.HeartRate)
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
    public void WorkoutBuilder_ShouldCreateDistanceIntervals()
    {
        // Arrange & Act
        var workout = new WorkoutBuilder()
            .Name("Distance Intervals")
            .Sport(Sport.Running)
            .DistanceInterval("400m Run", 400, 4, TargetType.Speed)
            .Build();

        // Assert
        Assert.Equal("Distance Intervals", workout.Name);
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
                           .TimeInterval("Effort", 10, 3, TargetType.Power)
                           .CoolDown(10);

        // Assert - Each method should return the same builder instance for chaining
        Assert.IsType<WorkoutBuilder>(result);
        
        // Build and verify the workout was properly constructed
        var workout = result.Build();
        Assert.Equal("Fluent Test", workout.Name);
        Assert.Equal(Sport.Cycling, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);
    }
}