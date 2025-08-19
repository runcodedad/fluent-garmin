# Fluent Garmin

[![Latest Release](https://img.shields.io/github/v/release/runcodedad/fluent-garmin?label=Latest%20Release)](https://github.com/runcodedad/fluent-garmin/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/runcodedad/fluent-garmin?include_prereleases&label=Latest%20Pre-release)](https://github.com/runcodedad/fluent-garmin/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Fluent.Garmin)](https://www.nuget.org/packages/Fluent.Garmin)

A .NET 8 fluent wrapper library for Garmin's FIT SDK, enabling easy creation of workout files for Garmin devices.

## Features

- **Fluent API**: Easy-to-use builder pattern for creating workouts
- **Full Garmin FIT SDK Integration**: Leverages the official Garmin.FIT.Sdk package
- **Proper Repeat Structure Support**: Uses Garmin's FIT SDK repeat patterns instead of manually creating individual steps
- **Multiple Target Types**: Support for Heart Rate, Speed, Power, and Cadence targets
- **Zone and Custom Range Support**: Use predefined zones or custom value ranges
- **Interval Creation**: Easy interval creation with automatic repeat structure using `AddIntervals()` method
- **Custom Repeat Patterns**: Flexible repeat structures with `AddRepeat()` method
- **Example Workouts**: Pre-built examples for common workout types
- **Comprehensive Testing**: Full test coverage with 23+ unit tests

## Quick Start

### Example 1: Basic Workout (Warm Up, Run, Cool Down)

```csharp
using Fluent.Garmin;
using Dynastream.Fit;

var workout = new WorkoutBuilder()
    .Name("Morning Run")
    .Sport(Sport.Running)
    .WarmUp(10, 1, TargetType.HeartRate)  // 10 minutes in HR zone 1
    .AddTimeStep("Main Set", 20, 4, TargetType.HeartRate)  // 20 minutes in HR zone 4
    .CoolDown(5, 1, TargetType.HeartRate)  // 5 minutes in HR zone 1
    .Build();

// Generate the FIT file
WorkoutGenerator.GenerateWorkoutFile(workout, "morning_run.fit");
```

### Example 2: Interval Workout with Repeat Structure

```csharp
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

var workout = new WorkoutBuilder()
    .Name("Track Intervals")
    .Sport(Sport.Running)
    .WarmUp(10, 1)  // 10 minutes in HR zone 1
    .AddIntervals("5x400m", 5, intervalOptions, recoveryOptions)
    .CoolDown(10, 1)  // 10 minutes in HR zone 1
    .Build();

WorkoutGenerator.GenerateWorkoutFile(workout, "intervals.fit");
```

### Example 3: Fartlek Workout (Mixed Intervals)

```csharp
var hardIntervalOptions = new IntervalOptions
{
    DurationType = DurationType.Time,
    Value = 180,
    Zone = 4,
    TargetType = TargetType.HeartRate
};

var hardRecoveryOptions = new RecoveryOptions
{
    DurationType = DurationType.Time,
    Value = 90,
    TargetType = TargetType.Open
};

var strideIntervalOptions = new IntervalOptions
{
    DurationType = DurationType.Time,
    Value = 30,
    Zone = 5,
    TargetType = TargetType.HeartRate
};

var strideRecoveryOptions = new RecoveryOptions
{
    DurationType = DurationType.Time,
    Value = 60,
    TargetType = TargetType.Open
};

var workout = new WorkoutBuilder()
    .Name("Fartlek Run")
    .Sport(Sport.Running)
    .WarmUp(15, 2, TargetType.HeartRate)  // 15 minutes easy
    .AddIntervals("4x3min Hard", 4, hardIntervalOptions, hardRecoveryOptions)
    .AddTimeStep("Steady State", 10, 3, TargetType.HeartRate)  // 10min moderate
    .AddIntervals("6x30sec Strides", 6, strideIntervalOptions, strideRecoveryOptions)
    .CoolDown(10, 1, TargetType.HeartRate)  // 10 minutes easy
    .Build();

WorkoutGenerator.GenerateWorkoutFile(workout, "fartlek.fit");
```

### Using Pre-built Example Workouts

```csharp
// 5x400m track intervals with proper repeat structure
var intervals = WorkoutGenerator.CreateIntervalWorkoutModel();
WorkoutGenerator.GenerateWorkoutFile(intervals, "intervals.fit");

// 20-minute tempo run
var tempo = WorkoutGenerator.CreateTempoRunModel();
WorkoutGenerator.GenerateWorkoutFile(tempo, "tempo.fit");

// Power-based cycling intervals with 3x Build/Recovery pattern
var cycling = WorkoutGenerator.CreateCyclingWorkoutModel();
WorkoutGenerator.GenerateWorkoutFile(cycling, "cycling.fit");
```

### Creating Advanced Repeat Patterns

```csharp
// Custom repeat structure with build and recovery phases
var buildStep = new WorkoutStep
{
    Name = "Build",
    Duration = new StepDuration { Type = DurationType.Time, Value = 300 }, // 5 min
    Intensity = Intensity.Active,
    Target = new StepTarget { Type = TargetType.Power, Zone = 3 }
};

var recoveryStep = new WorkoutStep
{
    Name = "Recovery", 
    Duration = new StepDuration { Type = DurationType.Time, Value = 180 }, // 3 min
    Intensity = Intensity.Rest,
    Target = new StepTarget { Type = TargetType.Power, Zone = 1 }
};

var workout = new WorkoutBuilder()
    .Name("Power Intervals")
    .Sport(Sport.Cycling)
    .WarmUp(15, 1, TargetType.Power)
    .AddRepeat("3x Build/Recovery", 3, buildStep, recoveryStep)
    .CoolDown(10, 1, TargetType.Power)
    .Build();
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

## Repeat Structure Support

The library uses proper Garmin FIT SDK repeat patterns for efficient interval creation:

### AddIntervals() Method
Creates structured intervals with automatic repeat functionality:

```csharp
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

.AddIntervals("5x400m", 5, intervalOptions, recoveryOptions)
```

This creates:
- **Repeat Step**: Parent step with `RepeatUntilStepsCmplt` duration type
- **Child Steps**: Interval and recovery steps contained within repeat structure  
- **Completion Step**: Defines how many times to repeat the pattern (5 times)

### AddRepeat() Method
For custom repeat patterns with specified child steps:

```csharp
.AddRepeat("Pyramid", 3, 
    new WorkoutStep { Name = "Build", Duration = ... },
    new WorkoutStep { Name = "Recover", Duration = ... })
```

### Benefits Over Manual Steps
- More efficient FIT file structure
- Follows Garmin's recommended practices
- Easier to modify repeat counts
- Better display on Garmin devices

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
- **RepeatUntilStepsCmplt**: Repeat until specified steps complete (used for repeat structures)
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