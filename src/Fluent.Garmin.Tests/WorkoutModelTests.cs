using Dynastream.Fit;
using Fluent.Garmin;

namespace Fluent.Garmin.Tests;

public class WorkoutModelTests
{
    [Fact]
    public void WorkoutModel_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var workout = new WorkoutModel();

        // Assert
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.NotNull(workout.Steps);
        Assert.Empty(workout.Steps);
    }

    [Fact]
    public void WorkoutModel_ShouldAllowSettingProperties()
    {
        // Arrange
        var workout = new WorkoutModel();
        var step = new WorkoutStep
        {
            Name = "Test Step",
            Duration = new StepDuration { Type = DurationType.Time, Value = 300 },
            Intensity = Intensity.Active,
            Target = new StepTarget { Type = TargetType.HeartRate, Zone = 3 }
        };

        // Act
        workout.Name = "Test Workout";
        workout.Sport = Sport.Cycling;
        workout.Steps.Add(step);

        // Assert
        Assert.Equal("Test Workout", workout.Name);
        Assert.Equal(Sport.Cycling, workout.Sport);
        Assert.Single(workout.Steps);
        Assert.Equal("Test Step", workout.Steps[0].Name);
    }
}