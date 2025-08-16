using Dynastream.Fit;

namespace Fluent.Garmin;

public class WorkoutModel
{
    public string? Name { get; set; }
    public Sport Sport { get; set; } = Sport.Running;
    public List<WorkoutStep> Steps { get; set; } = new List<WorkoutStep>();
}