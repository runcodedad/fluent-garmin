using Dynastream.Fit;

namespace Fluent.Garmin;

/// <summary>
/// Options for configuring an interval step
/// </summary>
public class IntervalOptions
{
    public DurationType DurationType { get; set; }
    public uint Value { get; set; }
    public uint Zone { get; set; }
    public TargetType TargetType { get; set; } = TargetType.Speed;
}

/// <summary>
/// Options for configuring a recovery step
/// </summary>
public class RecoveryOptions
{
    public DurationType DurationType { get; set; }
    public uint Value { get; set; }
    public uint? Zone { get; set; }
    public TargetType TargetType { get; set; } = TargetType.Open;
}