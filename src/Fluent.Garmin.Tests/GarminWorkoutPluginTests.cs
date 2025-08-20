using System.Text.Json;
using Dynastream.Fit;
using Fluent.Garmin.AI;

namespace Fluent.Garmin.Tests;

public class GarminWorkoutPluginTests
{
    private readonly GarminWorkoutPlugin _plugin = new();

    [Fact]
    public void CreateWorkoutFromJson_ShouldCreateBasicWorkout()
    {
        // Arrange
        var json = """
        {
            "name": "AI Generated Workout",
            "sport": "Running",
            "steps": [
                {
                    "name": "Warm Up",
                    "type": "warmup",
                    "duration": {
                        "type": "Time",
                        "value": 600
                    },
                    "target": {
                        "type": "HeartRate",
                        "zone": 1
                    },
                    "intensity": "Warmup"
                },
                {
                    "name": "Main Set",
                    "type": "step",
                    "duration": {
                        "type": "Time",
                        "value": 1800
                    },
                    "target": {
                        "type": "HeartRate",
                        "zone": 3
                    },
                    "intensity": "Active"
                }
            ]
        }
        """;

        // Act
        var workout = _plugin.CreateWorkoutFromJson(json);

        // Assert
        Assert.Equal("AI Generated Workout", workout.Name);
        Assert.Equal(Sport.Running, workout.Sport);
        Assert.Equal(2, workout.Steps.Count);
        
        var warmUp = workout.Steps[0];
        Assert.Equal("Warm Up", warmUp.Name);
        Assert.Equal(600u, warmUp.Duration?.Value);
        Assert.Equal(Intensity.Warmup, warmUp.Intensity);
        Assert.Equal(TargetType.HeartRate, warmUp.Target?.Type);
        Assert.Equal(1u, warmUp.Target?.Zone);
    }

    [Fact]
    public void CreateWorkoutFromJson_ShouldHandleRepeatStructure()
    {
        // Arrange
        var json = """
        {
            "name": "Interval Workout",
            "sport": "Running",
            "steps": [
                {
                    "name": "5x400m Intervals",
                    "type": "repeat",
                    "repeatCount": 5,
                    "repeatSteps": [
                        {
                            "name": "400m Effort",
                            "type": "step",
                            "duration": {
                                "type": "Distance",
                                "value": 400
                            },
                            "target": {
                                "type": "Speed",
                                "zone": 4
                            },
                            "intensity": "Active"
                        },
                        {
                            "name": "Recovery",
                            "type": "step",
                            "duration": {
                                "type": "Time",
                                "value": 120
                            },
                            "target": {
                                "type": "Open"
                            },
                            "intensity": "Rest"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var workout = _plugin.CreateWorkoutFromJson(json);

        // Assert
        Assert.Equal("Interval Workout", workout.Name);
        Assert.Single(workout.Steps);
        
        var intervalSet = workout.Steps[0];
        Assert.Equal("5x400m Intervals", intervalSet.Name);
        Assert.True(intervalSet.IsRepeat);
        Assert.Equal(5u, intervalSet.RepeatCount);
        Assert.Equal(2, intervalSet.RepeatSteps.Count);
        
        var effort = intervalSet.RepeatSteps[0];
        Assert.Equal("400m Effort", effort.Name);
        Assert.Equal(400u, effort.Duration?.Value);
        Assert.Equal(DurationType.Distance, effort.Duration?.Type);
    }

    [Fact]
    public void CreateWorkoutFromJson_ShouldThrowOnInvalidJson()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _plugin.CreateWorkoutFromJson(invalidJson));
    }

    [Fact]
    public void ValidateWorkoutPlan_ShouldReturnValidForGoodPlan()
    {
        // Arrange
        var json = """
        {
            "name": "Test Workout",
            "sport": "Running",
            "steps": [
                {
                    "name": "Main Set",
                    "type": "step",
                    "duration": {
                        "type": "Time",
                        "value": 1800
                    },
                    "target": {
                        "type": "HeartRate",
                        "zone": 3
                    }
                }
            ]
        }
        """;

        // Act
        var result = _plugin.ValidateWorkoutPlan(json);

        // Assert
        Assert.StartsWith("Valid:", result);
    }

    [Fact]
    public void ValidateWorkoutPlan_ShouldReturnInvalidForBadJson()
    {
        // Arrange
        var badJson = "{ invalid json }";

        // Act
        var result = _plugin.ValidateWorkoutPlan(badJson);

        // Assert
        Assert.StartsWith("Invalid:", result);
    }

    [Fact]
    public void ValidateWorkoutPlan_ShouldWarnOnEmptySteps()
    {
        // Arrange
        var json = """
        {
            "name": "Empty Workout",
            "sport": "Running",
            "steps": []
        }
        """;

        // Act
        var result = _plugin.ValidateWorkoutPlan(json);

        // Assert
        Assert.Contains("no steps", result);
    }

    [Fact]
    public void CreateWorkoutFile_ShouldCreateFitFile()
    {
        // Arrange
        var json = """
        {
            "name": "AI Test Workout",
            "sport": "Cycling",
            "steps": [
                {
                    "name": "Easy Ride",
                    "type": "step",
                    "duration": {
                        "type": "Time",
                        "value": 1200
                    },
                    "target": {
                        "type": "Power",
                        "zone": 2
                    }
                }
            ]
        }
        """;
        var fileName = "test-ai-workout";

        // Act
        var resultFileName = _plugin.CreateWorkoutFile(json, fileName);

        // Assert
        Assert.Equal("test-ai-workout.fit", resultFileName);
        Assert.True(System.IO.File.Exists(resultFileName));
        
        // Cleanup
        System.IO.File.Delete(resultFileName);
    }

    [Fact]
    public void WorkoutPlan_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var originalPlan = new WorkoutPlan
        {
            Name = "Test Plan",
            Sport = Sport.Cycling,
            Steps = new List<StepPlan>
            {
                new StepPlan
                {
                    Name = "Warm Up",
                    Type = "warmup",
                    Duration = new DurationPlan { Type = DurationType.Time, Value = 600 },
                    Target = new TargetPlan { Type = TargetType.HeartRate, Zone = 1 },
                    Intensity = Intensity.Warmup
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalPlan);
        var deserializedPlan = JsonSerializer.Deserialize<WorkoutPlan>(json);

        // Assert
        Assert.NotNull(deserializedPlan);
        Assert.Equal(originalPlan.Name, deserializedPlan.Name);
        Assert.Equal(originalPlan.Sport, deserializedPlan.Sport);
        Assert.Single(deserializedPlan.Steps);
        Assert.Equal(originalPlan.Steps[0].Name, deserializedPlan.Steps[0].Name);
    }

    [Fact]
    public void DurationPlan_MinutesProperty_ShouldConvertCorrectly()
    {
        // Arrange
        var duration = new DurationPlan();

        // Act
        duration.Minutes = 10;

        // Assert
        Assert.Equal(600u, duration.Value);
        Assert.Equal(10u, duration.Minutes);
    }
}