using Dynastream.Fit;

namespace Fluent.Garmin;

public class WorkoutStep
{
    public string? Name { get; set; }
    public StepDuration? Duration { get; set; }
    public Intensity Intensity { get; set; } = Intensity.Active;
    public StepTarget? Target { get; set; }
}