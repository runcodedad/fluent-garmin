# Fluent Garmin

A .NET 8 fluent wrapper library for Garmin's FIT SDK, enabling easy creation of workout files for Garmin devices.

## Features

- **Fluent API**: Easy-to-use builder pattern for creating workouts
- **Full Garmin FIT SDK Integration**: Leverages the official Garmin.FIT.Sdk package
- **Multiple Target Types**: Support for Heart Rate, Speed, Power, and Cadence targets
- **Zone and Custom Range Support**: Use predefined zones or custom value ranges
- **Example Workouts**: Pre-built examples for common workout types
- **Comprehensive Testing**: Full test coverage with 21+ unit tests

## Quick Start

### Creating a Simple Workout with the Fluent Builder

```csharp
using Fluent.Garmin;
using Dynastream.Fit;

var workout = new WorkoutBuilder()
    .Name("Morning Run")
    .Sport(Sport.Running)
    .WarmUp(10, 1)  // 10 minutes in HR zone 1
    .TimeInterval("Main Set", 20, 4, TargetType.HeartRate)  // 20 minutes in HR zone 4
    .CoolDown(5, 1)  // 5 minutes in HR zone 1
    .Build();

// Generate the FIT file
WorkoutGenerator.GenerateWorkoutFile(workout, "morning_run.fit");
```

### Using Pre-built Example Workouts

```csharp
// Interval training
var intervals = WorkoutGenerator.CreateIntervalWorkoutModel();
WorkoutGenerator.GenerateWorkoutFile(intervals, "intervals.fit");

// Tempo run
var tempo = WorkoutGenerator.CreateTempoRunModel();
WorkoutGenerator.GenerateWorkoutFile(tempo, "tempo.fit");

// Cycling workout
var cycling = WorkoutGenerator.CreateCyclingWorkoutModel();
WorkoutGenerator.GenerateWorkoutFile(cycling, "cycling.fit");
```

### Creating Custom Target Ranges

```csharp
var workout = new WorkoutModel
{
    Name = "Custom Targets",
    Sport = Sport.Running,
    Steps = new List<WorkoutStep>
    {
        new WorkoutStep
        {
            Name = "Heart Rate Range",
            Duration = new StepDuration { Type = DurationType.Time, Value = 1200 }, // 20 minutes
            Intensity = Intensity.Active,
            Target = new StepTarget 
            { 
                Type = TargetType.HeartRate, 
                LowValue = 150,   // Custom low value
                HighValue = 170   // Custom high value
            }
        }
    }
};
```

## Project Structure

- **Fluent.Garmin**: Main library containing all workout classes and the fluent API
- **Fluent.Garmin.Tests**: Comprehensive test suite with examples and edge cases

## Target Types

- **Open**: No specific target
- **HeartRate**: Heart rate zones (1-5) or custom BPM ranges
- **Speed**: Speed zones (1-5) or custom speed ranges
- **Power**: Power zones (1-5) or custom wattage ranges  
- **Cadence**: Custom cadence ranges (no predefined zones)

## Duration Types

- **Time**: Duration in seconds
- **Distance**: Distance in meters
- **Open**: No specific duration (manual advance)
- **Calories**: Target calorie burn
- **RepeatUntilStepsCmplt**: Repeat until specified steps complete
- **RepeatUntilTime**: Repeat until time duration
- **RepeatUntilDistance**: Repeat until distance covered

## Requirements

- .NET 8.0
- Garmin.FIT.Sdk package (automatically included)

## Installation

```bash
dotnet add package Fluent.Garmin
```

## Building from Source

```bash
git clone https://github.com/runcodedad/fluent-garmin.git
cd fluent-garmin
dotnet build
dotnet test
```