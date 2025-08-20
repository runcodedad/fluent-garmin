# AI Plugin Example

This is a simple console application that demonstrates how to use the Fluent Garmin AI plugin with Semantic Kernel.

## Setup

```bash
dotnet add package Fluent.Garmin
dotnet add package Microsoft.SemanticKernel
```

## Example Usage

```csharp
using Microsoft.SemanticKernel;
using Fluent.Garmin.AI;

// Create and configure Semantic Kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", "your-openai-api-key");
var kernel = builder.Build();

// Register the Garmin Workout Plugin
kernel.Plugins.AddFromType<GarminWorkoutPlugin>("garmin");

// Ask AI to create a workout
var prompt = """
Create a 30-minute running workout with:
- 5-minute warm-up in heart rate zone 1
- 20-minute main set alternating every 5 minutes between zones 3 and 4
- 5-minute cool-down in zone 1
Return as a JSON workout plan.
""";

var response = await kernel.InvokePromptAsync(prompt);
var jsonPlan = response.GetValue<string>();

// Create the workout file using the AI plugin
var createFileArgs = new KernelArguments 
{ 
    ["jsonPlan"] = jsonPlan,
    ["fileName"] = "ai-generated-workout.fit"
};

var result = await kernel.InvokeAsync("garmin", "CreateWorkoutFile", createFileArgs);
var filePath = result.GetValue<string>();

Console.WriteLine($"Workout file created: {filePath}");
```

## Manual JSON Example

You can also manually create JSON workout plans:

```csharp
var jsonWorkout = """
{
    "name": "Manual AI Workout",
    "sport": "Running",
    "steps": [
        {
            "name": "Warm Up",
            "type": "warmup",
            "duration": { "type": "Time", "value": 300 },
            "target": { "type": "HeartRate", "zone": 1 },
            "intensity": "Warmup"
        },
        {
            "name": "Main Set",
            "type": "step", 
            "duration": { "type": "Time", "value": 1200 },
            "target": { "type": "HeartRate", "zone": 3 },
            "intensity": "Active"
        }
    ]
}
""";

var plugin = new GarminWorkoutPlugin();
var workout = plugin.CreateWorkoutFromJson(jsonWorkout);
WorkoutGenerator.GenerateWorkoutFile(workout, "manual-workout.fit");
```