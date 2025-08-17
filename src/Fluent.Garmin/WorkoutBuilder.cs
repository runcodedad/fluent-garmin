using Dynastream.Fit;

namespace Fluent.Garmin;

/// <summary>
/// Builder pattern helper for creating workouts fluently
/// </summary>
public class WorkoutBuilder : IWorkoutBuilder
{
    private WorkoutModel workout = new WorkoutModel();

    public IWorkoutBuilder Name(string name)
    {
        workout.Name = name;
        return this;
    }

    public IWorkoutBuilder Sport(Sport sport)
    {
        workout.Sport = sport;
        return this;
    }

    public IWorkoutBuilder AddStep(string name, DurationType durationType, uint durationValue, 
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
    public IWorkoutBuilder WarmUp(uint minutes, uint? zone = 1, TargetType targetType = TargetType.HeartRate)
    {
        return AddStep("Warm Up", DurationType.Time, minutes * 60, Intensity.Warmup, targetType, zone);
    }

    public IWorkoutBuilder CoolDown(uint minutes, uint? zone = 1, TargetType targetType = TargetType.HeartRate)
    {
        return AddStep("Cool Down", DurationType.Time, minutes * 60, Intensity.Cooldown, targetType, zone);
    }

    public IWorkoutBuilder AddTimeStep(string name, uint minutes, uint? zone = null, TargetType targetType = TargetType.HeartRate)
    {
        return AddStep(name, DurationType.Time, minutes * 60, Intensity.Active, targetType, zone);
    }

    public IWorkoutBuilder AddDistanceStep(string name, uint meters, uint? zone = null, TargetType targetType = TargetType.Speed)
    {
        return AddStep(name, DurationType.Distance, meters, Intensity.Active, targetType, zone);
    }

    /// <summary>
    /// Adds a repeat structure for intervals (e.g., 5x400m with recovery)
    /// </summary>
    /// <param name="name">Name of the interval set</param>
    /// <param name="repeatCount">Number of times to repeat</param>
    /// <param name="intervalOptions">Configuration for the interval step</param>
    /// <param name="recoveryOptions">Configuration for the recovery step</param>
    public IWorkoutBuilder AddIntervals(string name, uint repeatCount, IntervalOptions intervalOptions, RecoveryOptions recoveryOptions)
    {
        var intervalStep = new WorkoutStep
        {
            Name = "Interval",
            Duration = new StepDuration { Type = intervalOptions.DurationType, Value = intervalOptions.Value },
            Intensity = Intensity.Active,
            Target = new StepTarget { Type = intervalOptions.TargetType, Zone = intervalOptions.Zone }
        };

        var recoveryStep = new WorkoutStep
        {
            Name = "Recovery",
            Duration = new StepDuration { Type = recoveryOptions.DurationType, Value = recoveryOptions.Value },
            Intensity = Intensity.Rest,
            Target = new StepTarget { Type = recoveryOptions.TargetType, Zone = recoveryOptions.Zone }
        };

        var repeatStep = new WorkoutStep
        {
            Name = name,
            IsRepeat = true,
            RepeatCount = repeatCount,
            Duration = new StepDuration { Type = DurationType.RepeatUntilStepsCmplt, Value = repeatCount },
            Intensity = Intensity.Active,
            RepeatSteps = new List<WorkoutStep> { intervalStep, recoveryStep }
        };

        workout.Steps.Add(repeatStep);
        return this;
    }



    /// <summary>
    /// Adds a custom repeat structure with specified child steps
    /// </summary>
    /// <param name="name">Name of the repeat</param>
    /// <param name="repeatCount">Number of times to repeat</param>
    /// <param name="steps">Child steps to repeat</param>
    public IWorkoutBuilder AddRepeat(string name, uint repeatCount, params WorkoutStep[] steps)
    {
        var repeatStep = new WorkoutStep
        {
            Name = name,
            IsRepeat = true,
            RepeatCount = repeatCount,
            Duration = new StepDuration { Type = DurationType.RepeatUntilStepsCmplt, Value = repeatCount },
            Intensity = Intensity.Active,
            RepeatSteps = new List<WorkoutStep>(steps)
        };

        workout.Steps.Add(repeatStep);
        return this;
    }
}