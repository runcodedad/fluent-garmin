using System.Text.Json;
using Fluent.Garmin.AI;

namespace Fluent.Garmin.Tests;

// This is a demonstration of the AI plugin working end-to-end
public class AIPluginDemoTests
{
    [Fact]
    public void Demo_CompleteAIWorkflow_ShouldWork()
    {
        // Arrange - Simulate what an AI would generate
        var aiGeneratedJson = """
        {
            "name": "AI-Generated Tempo Run",
            "sport": "Running",
            "steps": [
                {
                    "name": "Easy Warm-Up",
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
                    "name": "Tempo Effort",
                    "type": "step",
                    "duration": {
                        "type": "Time", 
                        "value": 1200
                    },
                    "target": {
                        "type": "HeartRate",
                        "zone": 4
                    },
                    "intensity": "Active"
                },
                {
                    "name": "Easy Cool-Down",
                    "type": "cooldown",
                    "duration": {
                        "type": "Time",
                        "value": 600
                    },
                    "target": {
                        "type": "HeartRate",
                        "zone": 1
                    },
                    "intensity": "Cooldown"
                }
            ]
        }
        """;

        var plugin = new GarminWorkoutPlugin();

        // Act - Demonstrate the complete workflow
        
        // 1. Validate the AI-generated plan
        var validation = plugin.ValidateWorkoutPlan(aiGeneratedJson);
        Assert.StartsWith("Valid:", validation);

        // 2. Create workout model from JSON
        var workoutModel = plugin.CreateWorkoutFromJson(aiGeneratedJson);
        Assert.Equal("AI-Generated Tempo Run", workoutModel.Name);
        Assert.Equal(3, workoutModel.Steps.Count);

        // 3. Generate FIT file
        var fileName = "demo-ai-workout.fit";
        var resultFile = plugin.CreateWorkoutFile(aiGeneratedJson, fileName);
        Assert.Equal(fileName, resultFile);
        Assert.True(System.IO.File.Exists(fileName));

        // 4. Verify the generated FIT file can be loaded/validated
        // (This would be done by Garmin devices/Connect IQ)
        var fileInfo = new System.IO.FileInfo(fileName);
        Assert.True(fileInfo.Length > 0);

        // Cleanup
        System.IO.File.Delete(fileName);
    }

    [Fact]
    public void Demo_ComplexIntervalWorkout_ShouldWork()
    {
        // Arrange - Complex workout with intervals
        var complexJson = """
        {
            "name": "Advanced Cycling Intervals",
            "sport": "Cycling",
            "steps": [
                {
                    "name": "Warm-Up",
                    "type": "warmup",
                    "duration": {
                        "type": "Time",
                        "value": 900
                    },
                    "target": {
                        "type": "Power",
                        "zone": 1
                    },
                    "intensity": "Warmup"
                },
                {
                    "name": "4x5min VO2 Max Intervals",
                    "type": "repeat",
                    "repeatCount": 4,
                    "repeatSteps": [
                        {
                            "name": "VO2 Max Effort",
                            "type": "step",
                            "duration": {
                                "type": "Time",
                                "value": 300
                            },
                            "target": {
                                "type": "Power",
                                "zone": 5
                            },
                            "intensity": "Active"
                        },
                        {
                            "name": "Recovery",
                            "type": "step",
                            "duration": {
                                "type": "Time",
                                "value": 240
                            },
                            "target": {
                                "type": "Power",
                                "zone": 1
                            },
                            "intensity": "Rest"
                        }
                    ]
                },
                {
                    "name": "Cool-Down",
                    "type": "cooldown",
                    "duration": {
                        "type": "Time",
                        "value": 600
                    },
                    "target": {
                        "type": "Power",
                        "zone": 1
                    },
                    "intensity": "Cooldown"
                }
            ]
        }
        """;

        var plugin = new GarminWorkoutPlugin();

        // Act & Assert
        var workout = plugin.CreateWorkoutFromJson(complexJson);
        
        Assert.Equal("Advanced Cycling Intervals", workout.Name);
        Assert.Equal(Dynastream.Fit.Sport.Cycling, workout.Sport);
        Assert.Equal(3, workout.Steps.Count);
        
        // Check the interval structure
        var intervalStep = workout.Steps[1];
        Assert.True(intervalStep.IsRepeat);
        Assert.Equal(4u, intervalStep.RepeatCount);
        Assert.Equal(2, intervalStep.RepeatSteps.Count);
        
        // Verify the interval and recovery steps
        var effortStep = intervalStep.RepeatSteps[0];
        Assert.Equal("VO2 Max Effort", effortStep.Name);
        Assert.Equal(300u, effortStep.Duration?.Value);
        Assert.Equal(TargetType.Power, effortStep.Target?.Type);
        Assert.Equal(5u, effortStep.Target?.Zone);
    }
}