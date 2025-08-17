using Dynastream.Fit;

namespace Fluent.Garmin;

/// <summary>
/// Interface for building workouts fluently, supporting dependency injection
/// </summary>
public interface IWorkoutBuilder
{
    /// <summary>
    /// Sets the name of the workout
    /// </summary>
    /// <param name="name">The workout name</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder Name(string name);

    /// <summary>
    /// Sets the sport for the workout
    /// </summary>
    /// <param name="sport">The sport type</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder Sport(Sport sport);

    /// <summary>
    /// Adds a generic workout step
    /// </summary>
    /// <param name="name">Step name</param>
    /// <param name="durationType">Duration type</param>
    /// <param name="durationValue">Duration value</param>
    /// <param name="intensity">Step intensity</param>
    /// <param name="targetType">Target type</param>
    /// <param name="targetZone">Target zone</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder AddStep(string name, DurationType durationType, uint durationValue, 
                            Intensity intensity = Intensity.Active, 
                            TargetType targetType = TargetType.Open, 
                            uint? targetZone = null);

    /// <summary>
    /// Adds a warm-up step
    /// </summary>
    /// <param name="minutes">Duration in minutes</param>
    /// <param name="zone">Target zone</param>
    /// <param name="targetType">Target type</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder WarmUp(uint minutes, uint? zone = 1, TargetType targetType = TargetType.HeartRate);

    /// <summary>
    /// Adds a cool-down step
    /// </summary>
    /// <param name="minutes">Duration in minutes</param>
    /// <param name="zone">Target zone</param>
    /// <param name="targetType">Target type</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder CoolDown(uint minutes, uint? zone = 1, TargetType targetType = TargetType.HeartRate);

    /// <summary>
    /// Adds a time-based workout step
    /// </summary>
    /// <param name="name">Step name</param>
    /// <param name="minutes">Duration in minutes</param>
    /// <param name="zone">Target zone</param>
    /// <param name="targetType">Target type</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder AddTimeStep(string name, uint minutes, uint? zone = null, TargetType targetType = TargetType.HeartRate);

    /// <summary>
    /// Adds a distance-based workout step
    /// </summary>
    /// <param name="name">Step name</param>
    /// <param name="meters">Distance in meters</param>
    /// <param name="zone">Target zone</param>
    /// <param name="targetType">Target type</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder AddDistanceStep(string name, uint meters, uint? zone = null, TargetType targetType = TargetType.Speed);

    /// <summary>
    /// Adds a repeat structure for intervals (e.g., 5x400m with recovery)
    /// </summary>
    /// <param name="name">Name of the interval set</param>
    /// <param name="repeatCount">Number of times to repeat</param>
    /// <param name="intervalOptions">Configuration for the interval step</param>
    /// <param name="recoveryOptions">Configuration for the recovery step</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder AddIntervals(string name, uint repeatCount, IntervalOptions intervalOptions, RecoveryOptions recoveryOptions);

    /// <summary>
    /// Adds a custom repeat structure with specified child steps
    /// </summary>
    /// <param name="name">Name of the repeat</param>
    /// <param name="repeatCount">Number of times to repeat</param>
    /// <param name="steps">Child steps to repeat</param>
    /// <returns>The builder instance for fluent chaining</returns>
    IWorkoutBuilder AddRepeat(string name, uint repeatCount, params WorkoutStep[] steps);

    /// <summary>
    /// Builds the final workout model
    /// </summary>
    /// <returns>The completed workout model</returns>
    WorkoutModel Build();
}