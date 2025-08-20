# AI Plugin Complete Example

This is a comprehensive example demonstrating how to use the Fluent Garmin AI plugin with Semantic Kernel to create workout files from natural language descriptions.

## Prerequisites

Ensure you have the following packages installed:

```bash
dotnet add package Fluent.Garmin
dotnet add package Microsoft.SemanticKernel
```

You'll also need an OpenAI API key or Azure OpenAI credentials.

## Complete Working Example

```csharp
using Microsoft.SemanticKernel;
using Fluent.Garmin.AI;
using Fluent.Garmin;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Setup Semantic Kernel with AI provider
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4", "your-openai-api-key");
        var kernel = builder.Build();

        // 2. Register the Garmin Workout Plugin
        kernel.Plugins.AddFromType<GarminWorkoutPlugin>("garmin");

        Console.WriteLine("Fluent Garmin AI Plugin Demo");
        Console.WriteLine("=============================\n");

        // Example 1: Let AI choose the function automatically
        await AutomaticFunctionSelection(kernel);

        // Example 2: Direct plugin function calls
        await DirectPluginCalls(kernel);

        // Example 3: Manual JSON creation
        await ManualJsonExample();

        // Example 4: Validation example
        await ValidationExample(kernel);
    }

    /// <summary>
    /// Example 1: Let Semantic Kernel automatically choose which function to call
    /// </summary>
    static async Task AutomaticFunctionSelection(Kernel kernel)
    {
        Console.WriteLine("=== Example 1: Automatic Function Selection ===");
        
        var prompt = """
        Create a 30-minute running workout with:
        - 5-minute warm-up in heart rate zone 1
        - 20-minute main set alternating every 5 minutes between zones 3 and 4
        - 5-minute cool-down in zone 1
        
        Create the workout file and name it "auto-generated-workout.fit"
        """;

        try
        {
            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine($"Result: {result.GetValue<string>()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Direct calls to specific plugin functions
    /// </summary>
    static async Task DirectPluginCalls(Kernel kernel)
    {
        Console.WriteLine("=== Example 2: Direct Plugin Function Calls ===");
        
        // Step 1: Generate JSON plan using AI
        var planPrompt = """
        Create a 25-minute cycling workout with:
        - 5-minute warm-up in zone 1
        - 15-minute interval set: 3x(3 minutes zone 4, 2 minutes zone 2)
        - 5-minute cool-down in zone 1
        
        Return ONLY the JSON workout plan.
        """;

        try
        {
            // Get AI-generated JSON plan
            var aiResponse = await kernel.InvokePromptAsync(planPrompt);
            var jsonPlan = aiResponse.GetValue<string>();
            
            Console.WriteLine("Generated JSON Plan:");
            Console.WriteLine(jsonPlan);
            Console.WriteLine();

            // Step 2: Create workout from JSON
            var workoutResult = await kernel.InvokeAsync("garmin", "CreateWorkoutFromJson", 
                new KernelArguments { ["jsonPlan"] = jsonPlan });
            
            var workout = workoutResult.GetValue<WorkoutModel>();
            Console.WriteLine($"Created workout: {workout.Name}");

            // Step 3: Generate FIT file
            var fileResult = await kernel.InvokeAsync("garmin", "CreateWorkoutFile", 
                new KernelArguments { 
                    ["jsonPlan"] = jsonPlan,
                    ["fileName"] = "cycling-intervals.fit"
                });
            
            var filePath = fileResult.GetValue<string>();
            Console.WriteLine($"Workout file created: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Manual JSON creation without AI
    /// </summary>
    static async Task ManualJsonExample()
    {
        Console.WriteLine("=== Example 3: Manual JSON Creation ===");
        
        var jsonWorkout = """
        {
            "name": "Manual Swimming Workout",
            "sport": "Swimming",
            "steps": [
                {
                    "name": "Warm Up",
                    "type": "warmup",
                    "duration": { "type": "Time", "value": 300 },
                    "target": { "type": "Open" },
                    "intensity": "Warmup"
                },
                {
                    "name": "Main Set",
                    "type": "repeat",
                    "repeatCount": 4,
                    "repeatSteps": [
                        {
                            "name": "100m Fast",
                            "type": "step",
                            "duration": { "type": "Distance", "value": 100 },
                            "target": { "type": "Open" },
                            "intensity": "Active"
                        },
                        {
                            "name": "50m Easy",
                            "type": "step", 
                            "duration": { "type": "Distance", "value": 50 },
                            "target": { "type": "Open" },
                            "intensity": "Rest"
                        }
                    ]
                },
                {
                    "name": "Cool Down",
                    "type": "cooldown",
                    "duration": { "type": "Time", "value": 300 },
                    "target": { "type": "Open" },
                    "intensity": "Cooldown"
                }
            ]
        }
        """;

        try
        {
            var plugin = new GarminWorkoutPlugin();
            var workout = plugin.CreateWorkoutFromJson(jsonWorkout);
            
            Console.WriteLine($"Created workout: {workout.Name}");
            Console.WriteLine($"Sport: {workout.Sport}");
            Console.WriteLine($"Steps: {workout.Steps.Count}");
            
            // Generate file
            WorkoutGenerator.GenerateWorkoutFile(workout, "manual-swimming.fit");
            Console.WriteLine("Manual workout file created: manual-swimming.fit");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: JSON validation
    /// </summary>
    static async Task ValidationExample(Kernel kernel)
    {
        Console.WriteLine("=== Example 4: JSON Validation ===");
        
        var invalidJson = """
        {
            "name": "",
            "sport": "Running",
            "steps": []
        }
        """;

        try
        {
            var validationResult = await kernel.InvokeAsync("garmin", "ValidateWorkoutPlan", 
                new KernelArguments { ["jsonPlan"] = invalidJson });
            
            var result = validationResult.GetValue<string>();
            Console.WriteLine($"Validation result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation error: {ex.Message}");
        }
        
        Console.WriteLine();
    }
}
```

## JSON Schema Reference

The plugin expects workout plans in this JSON format:

```json
{
  "name": "Workout Name",
  "sport": "Running|Cycling|Swimming|Generic",
  "steps": [
    {
      "name": "Step Description", 
      "type": "step|warmup|cooldown|repeat",
      "duration": {
        "type": "Time|Distance|Open|Calories",
        "value": "number (seconds for Time, meters for Distance)"
      },
      "target": {
        "type": "Open|HeartRate|Speed|Power|Cadence",
        "zone": "number 1-5 (optional, for predefined zones)",
        "lowValue": "number (optional, for custom ranges)",
        "highValue": "number (optional, for custom ranges)"
      },
      "intensity": "Active|Rest|Warmup|Cooldown",
      "repeatCount": "number (for repeat steps only)",
      "repeatSteps": [
        "... nested steps for repeat type ..."
      ]
    }
  ]
}
```

## Key Features

1. **Natural Language Processing**: Describe workouts in plain English
2. **Multiple Invocation Methods**: Auto-selection or direct function calls
3. **Comprehensive Validation**: Built-in JSON validation with helpful error messages
4. **Flexible Duration Types**: Time, distance, open-ended, or calorie-based
5. **Multiple Target Types**: Heart rate zones, speed, power, cadence
6. **Repeat Structures**: Complex interval and repeat patterns
7. **All Sports Supported**: Running, cycling, swimming, and generic workouts

## Error Handling

The plugin includes comprehensive error handling:

- JSON parsing validation
- Workout structure validation  
- Missing required fields detection
- Invalid duration/target combinations
- Helpful error messages for debugging

## Integration Tips

1. **Prompt Engineering**: Be specific about workout structure and zones
2. **Validation First**: Use ValidateWorkoutPlan before creating files
3. **Error Handling**: Always wrap calls in try-catch blocks
4. **File Management**: Specify .fit extension for generated files
5. **Testing**: Start with simple workouts before complex intervals