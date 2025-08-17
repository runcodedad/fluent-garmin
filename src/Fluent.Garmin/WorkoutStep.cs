using Dynastream.Fit;

namespace Fluent.Garmin;

public class WorkoutStep
{
    public string? Name { get; set; }
    public StepDuration? Duration { get; set; }
    public Intensity Intensity { get; set; } = Intensity.Active;
    public StepTarget? Target { get; set; }
    
    // Repeat structure properties
    public bool IsRepeat { get; set; } = false;
    public uint RepeatCount { get; set; } = 1;
    public List<WorkoutStep> RepeatSteps { get; set; } = new List<WorkoutStep>();
}