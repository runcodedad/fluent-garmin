using Dynastream.Fit;

namespace Fluent.Garmin;

/// <summary>
/// Builder pattern helper for creating workouts fluently
/// </summary>
public class WorkoutBuilder
{
    private WorkoutModel workout = new WorkoutModel();

    public WorkoutBuilder Name(string name)
    {
        workout.Name = name;
        return this;
    }

    public WorkoutBuilder Sport(Sport sport)
    {
        workout.Sport = sport;
        return this;
    }

    public WorkoutBuilder AddStep(string name, DurationType durationType, uint durationValue, 
                                Intensity intensity = Intensity.Active, 
                                TargetType targetType = TargetType.Open, 
                                uint? targetZone = null)
    {
        workout.Steps.Add(new WorkoutStep
        {
            Name = name,
            Duration = new StepDuration { Type = durationType, Value = durationValue },
            Intensity = intensity,
            Target = new StepTarget { Type = targetType, Zone = targetZone }
        });
        return this;
    }

    public WorkoutModel Build() => workout;

    // Quick helper methods
    public WorkoutBuilder WarmUp(uint minutes, uint? hrZone = 1)
    {
        return AddStep("Warm Up", DurationType.Time, minutes * 60, Intensity.Warmup, TargetType.HeartRate, hrZone);
    }

    public WorkoutBuilder CoolDown(uint minutes, uint? hrZone = 1)
    {
        return AddStep("Cool Down", DurationType.Time, minutes * 60, Intensity.Cooldown, TargetType.HeartRate, hrZone);
    }

    public WorkoutBuilder TimeInterval(string name, uint minutes, uint? zone = null, TargetType targetType = TargetType.HeartRate)
    {
        return AddStep(name, DurationType.Time, minutes * 60, Intensity.Active, targetType, zone);
    }

    public WorkoutBuilder DistanceInterval(string name, uint meters, uint? zone = null, TargetType targetType = TargetType.Speed)
    {
        return AddStep(name, DurationType.Distance, meters, Intensity.Active, targetType, zone);
    }
}