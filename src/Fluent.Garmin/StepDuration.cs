namespace Fluent.Garmin;

public class StepDuration
{
    public DurationType Type { get; set; }
    public uint Value { get; set; } // Time in seconds, Distance in meters, or 0 for Open
}