using System.Text.Json.Serialization;
using Dynastream.Fit;

namespace Fluent.Garmin.AI;

/// <summary>
/// JSON-friendly model for AI-generated workout plans
/// </summary>
public class WorkoutPlan
{
    /// <summary>
    /// Name of the workout
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sport type (Running, Cycling, Swimming, etc.)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Sport Sport { get; set; } = Sport.Running;

    /// <summary>
    /// List of workout steps
    /// </summary>
    public List<StepPlan> Steps { get; set; } = new();
}

/// <summary>
/// JSON-friendly model for individual workout steps
/// </summary>
public class StepPlan
{
    /// <summary>
    /// Name/description of the step
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of step (warmup, interval, recovery, cooldown, repeat)
    /// </summary>
    public string Type { get; set; } = "step";

    /// <summary>
    /// Duration configuration
    /// </summary>
    public DurationPlan Duration { get; set; } = new();

    /// <summary>
    /// Target configuration (heart rate, pace, power, etc.)
    /// </summary>
    public TargetPlan Target { get; set; } = new();

    /// <summary>
    /// Intensity level
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Intensity Intensity { get; set; } = Intensity.Active;

    /// <summary>
    /// For repeat steps: number of repetitions
    /// </summary>
    public uint RepeatCount { get; set; } = 1;

    /// <summary>
    /// For repeat steps: child steps to repeat
    /// </summary>
    public List<StepPlan> RepeatSteps { get; set; } = new();
}

/// <summary>
/// JSON-friendly model for step duration
/// </summary>
public class DurationPlan
{
    /// <summary>
    /// Duration type (time, distance, open, calories)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DurationType Type { get; set; } = DurationType.Time;

    /// <summary>
    /// Duration value (seconds for time, meters for distance, etc.)
    /// </summary>
    public uint Value { get; set; }

    /// <summary>
    /// Helper property for time in minutes (converted to seconds)
    /// </summary>
    [JsonIgnore]
    public uint Minutes
    {
        get => Value / 60;
        set => Value = value * 60;
    }
}

/// <summary>
/// JSON-friendly model for step targets
/// </summary>
public class TargetPlan
{
    /// <summary>
    /// Target type (open, heartrate, speed, power, cadence)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TargetType Type { get; set; } = TargetType.Open;

    /// <summary>
    /// Target zone (1-5 for predefined zones)
    /// </summary>
    public uint? Zone { get; set; }

    /// <summary>
    /// Custom low value for target range
    /// </summary>
    public uint? LowValue { get; set; }

    /// <summary>
    /// Custom high value for target range
    /// </summary>
    public uint? HighValue { get; set; }
}