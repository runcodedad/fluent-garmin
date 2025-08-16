namespace Fluent.Garmin;

public class StepTarget
{
    public TargetType Type { get; set; }
    public uint? Zone { get; set; } // For HR/Speed/Power zones (1-5)
    public uint? LowValue { get; set; } // For custom ranges
    public uint? HighValue { get; set; } // For custom ranges
}