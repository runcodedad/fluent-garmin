using Dynastream.Fit;

namespace Fluent.Garmin;

public static class WorkoutGenerator
{
    /// <summary>
    /// Generates a .fit workout file from a WorkoutModel
    /// </summary>
    /// <param name="workout">The workout model containing all workout details</param>
    /// <param name="filename">Output filename for the .fit file</param>
    public static void GenerateWorkoutFile(WorkoutModel workout, string filename)
    {
        if (workout == null || workout.Steps == null || workout.Steps.Count == 0)
        {
            throw new ArgumentException("Workout must have at least one step");
        }

        Encode encodeDemo = new Encode(ProtocolVersion.V10);
        FileStream fitDest = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        try
        {
            // Write FIT header
            encodeDemo.Open(fitDest);

            // Create file ID message
            var fileIdMesg = new FileIdMesg();
            fileIdMesg.SetType(Dynastream.Fit.File.Workout);
            fileIdMesg.SetManufacturer(Manufacturer.Development);
            fileIdMesg.SetProduct(0);
            fileIdMesg.SetTimeCreated(new Dynastream.Fit.DateTime((uint)System.DateTime.Now.Ticks));
            encodeDemo.Write(fileIdMesg);

            // Create workout message
            var workoutMesg = new WorkoutMesg();
            workoutMesg.SetWktName(workout.Name ?? "Custom Workout");
            workoutMesg.SetSport(workout.Sport);
            workoutMesg.SetCapabilities(32);
            
            // Calculate total number of steps including repeat child steps
            var totalSteps = CalculateTotalSteps(workout.Steps);
            workoutMesg.SetNumValidSteps((ushort)totalSteps);
            encodeDemo.Write(workoutMesg);

            // Create workout steps
            ushort stepIndex = 0;
            WriteWorkoutSteps(encodeDemo, workout.Steps, ref stepIndex);

            Console.WriteLine($"Workout '{workout.Name}' created successfully as '{filename}'!");
            Console.WriteLine($"Steps: {workout.Steps.Count}, Sport: {workout.Sport}");
        }
        finally
        {
            encodeDemo.Close();
            fitDest.Close();
        }
    }

    private static void SetStepDuration(WorkoutStepMesg workoutStep, StepDuration? duration)
    {
        if (duration == null)
        {
            workoutStep.SetDurationType(WktStepDuration.Open);
            return;
        }

        switch (duration.Type)
        {
            case DurationType.Time:
                workoutStep.SetDurationType(WktStepDuration.Time);
                workoutStep.SetDurationTime(duration.Value * 1000); // Convert seconds to milliseconds
                break;

            case DurationType.Distance:
                workoutStep.SetDurationType(WktStepDuration.Distance);
                workoutStep.SetDurationDistance(duration.Value * 100); // Convert meters to centimeters
                break;

            case DurationType.Calories:
                workoutStep.SetDurationType(WktStepDuration.Calories);
                workoutStep.SetDurationCalories(duration.Value);
                break;

            case DurationType.RepeatUntilStepsCmplt:
                workoutStep.SetDurationType(WktStepDuration.RepeatUntilStepsCmplt);
                workoutStep.SetDurationStep(duration.Value);
                break;

            case DurationType.RepeatUntilTime:
                workoutStep.SetDurationType(WktStepDuration.RepeatUntilTime);
                workoutStep.SetDurationTime(duration.Value * 1000);
                break;

            case DurationType.RepeatUntilDistance:
                workoutStep.SetDurationType(WktStepDuration.RepeatUntilDistance);
                workoutStep.SetDurationDistance(duration.Value * 100);
                break;

            case DurationType.Open:
            default:
                workoutStep.SetDurationType(WktStepDuration.Open);
                break;
        }
    }

    private static void SetStepTarget(WorkoutStepMesg workoutStep, StepTarget? target)
    {
        if (target == null)
        {
            workoutStep.SetTargetType(WktStepTarget.Open);
            return;
        }

        switch (target.Type)
        {
            case TargetType.HeartRate:
                workoutStep.SetTargetType(WktStepTarget.HeartRate);
                if (target.Zone.HasValue)
                {
                    workoutStep.SetTargetHrZone(target.Zone.Value);
                }
                else if (target.LowValue.HasValue && target.HighValue.HasValue)
                {
                    workoutStep.SetCustomTargetHeartRateLow(target.LowValue.Value);
                    workoutStep.SetCustomTargetHeartRateHigh(target.HighValue.Value);
                }
                break;

            case TargetType.Speed:
                workoutStep.SetTargetType(WktStepTarget.Speed);
                if (target.Zone.HasValue)
                {
                    workoutStep.SetTargetSpeedZone(target.Zone.Value);
                }
                else if (target.LowValue.HasValue && target.HighValue.HasValue)
                {
                    workoutStep.SetCustomTargetSpeedLow(target.LowValue.Value);
                    workoutStep.SetCustomTargetSpeedHigh(target.HighValue.Value);
                }
                break;

            case TargetType.Power:
                workoutStep.SetTargetType(WktStepTarget.Power);
                if (target.Zone.HasValue)
                {
                    workoutStep.SetTargetPowerZone(target.Zone.Value);
                }
                else if (target.LowValue.HasValue && target.HighValue.HasValue)
                {
                    workoutStep.SetCustomTargetPowerLow(target.LowValue.Value);
                    workoutStep.SetCustomTargetPowerHigh(target.HighValue.Value);
                }
                break;

            case TargetType.Cadence:
                workoutStep.SetTargetType(WktStepTarget.Cadence);
                if (target.LowValue.HasValue && target.HighValue.HasValue)
                {
                    workoutStep.SetCustomTargetCadenceLow(target.LowValue.Value);
                    workoutStep.SetCustomTargetCadenceHigh(target.HighValue.Value);
                }
                break;

            case TargetType.Open:
            default:
                workoutStep.SetTargetType(WktStepTarget.Open);
                break;
        }
    }

    private static int CalculateTotalSteps(List<WorkoutStep> steps)
    {
        int totalSteps = 0;
        foreach (var step in steps)
        {
            if (step.IsRepeat)
            {
                // Repeat step + child steps + completion step
                totalSteps += 1 + step.RepeatSteps.Count + 1;
            }
            else
            {
                totalSteps += 1;
            }
        }
        return totalSteps;
    }

    private static void WriteWorkoutSteps(Encode encoder, List<WorkoutStep> steps, ref ushort stepIndex)
    {
        foreach (var step in steps)
        {
            if (step.IsRepeat)
            {
                WriteRepeatStep(encoder, step, ref stepIndex);
            }
            else
            {
                WriteRegularStep(encoder, step, stepIndex);
                stepIndex++;
            }
        }
    }

    private static void WriteRepeatStep(Encode encoder, WorkoutStep repeatStep, ref ushort stepIndex)
    {
        // Create the parent repeat step
        var workoutStep = new WorkoutStepMesg();
        workoutStep.SetMessageIndex(stepIndex++);
        workoutStep.SetWktStepName(repeatStep.Name ?? "Repeat");
        workoutStep.SetIntensity(repeatStep.Intensity);
        SetStepDuration(workoutStep, repeatStep.Duration);
        SetStepTarget(workoutStep, repeatStep.Target);
        encoder.Write(workoutStep);

        // Write child steps
        foreach (var childStep in repeatStep.RepeatSteps)
        {
            WriteRegularStep(encoder, childStep, stepIndex);
            stepIndex++;
        }

        // Create the completion step that defines the repeat count
        var completionStep = new WorkoutStepMesg();
        completionStep.SetMessageIndex(stepIndex++);
        completionStep.SetWktStepName("Repeat Complete");
        completionStep.SetDurationType(WktStepDuration.RepeatUntilStepsCmplt);
        completionStep.SetDurationStep(repeatStep.RepeatCount);
        completionStep.SetIntensity(Intensity.Active);
        completionStep.SetTargetType(WktStepTarget.Open);
        encoder.Write(completionStep);
    }

    private static void WriteRegularStep(Encode encoder, WorkoutStep step, ushort stepIndex)
    {
        var workoutStep = new WorkoutStepMesg();
        workoutStep.SetMessageIndex(stepIndex);
        workoutStep.SetWktStepName(step.Name ?? $"Step {stepIndex + 1}");
        workoutStep.SetIntensity(step.Intensity);
        SetStepDuration(workoutStep, step.Duration);
        SetStepTarget(workoutStep, step.Target);
        encoder.Write(workoutStep);
    }

    // Example workout models
    public static WorkoutModel CreateIntervalWorkoutModel()
    {
        return new WorkoutBuilder()
            .Name("5x400m Track Intervals")
            .Sport(Sport.Running)
            .WarmUp(10, 1)  // 10 minutes in HR zone 1
            .AddIntervals("5x400m", 5, 
                DurationType.Distance, 400, 4,     // 400m at speed zone 4
                DurationType.Time, 120)            // 2min recovery
            .CoolDown(10, 1) // 10 minutes in HR zone 1
            .Build();
    }

    public static WorkoutModel CreateTempoRunModel()
    {
        return new WorkoutModel
        {
            Name = "20 Minute Tempo Run",
            Sport = Sport.Running,
            Steps = new List<WorkoutStep>
            {
                new WorkoutStep
                {
                    Name = "Warm Up",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 900 }, // 15 min
                    Intensity = Intensity.Warmup,
                    Target = new StepTarget { Type = TargetType.HeartRate, Zone = 2 }
                },
                new WorkoutStep
                {
                    Name = "Tempo",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 1200 }, // 20 min
                    Intensity = Intensity.Active,
                    Target = new StepTarget { Type = TargetType.HeartRate, Zone = 4 }
                },
                new WorkoutStep
                {
                    Name = "Cool Down",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 600 }, // 10 min
                    Intensity = Intensity.Cooldown,
                    Target = new StepTarget { Type = TargetType.HeartRate, Zone = 1 }
                }
            }
        };
    }

    public static WorkoutModel CreateCyclingWorkoutModel()
    {
        var buildStep = new WorkoutStep
        {
            Name = "Build",
            Duration = new StepDuration { Type = DurationType.Time, Value = 300 }, // 5 min
            Intensity = Intensity.Active,
            Target = new StepTarget { Type = TargetType.Power, Zone = 3 }
        };

        var recoveryStep = new WorkoutStep
        {
            Name = "Recovery",
            Duration = new StepDuration { Type = DurationType.Time, Value = 180 }, // 3 min
            Intensity = Intensity.Rest,
            Target = new StepTarget { Type = TargetType.Power, Zone = 1 }
        };

        return new WorkoutBuilder()
            .Name("Power Intervals")
            .Sport(Sport.Cycling)
            .WarmUp(15, 1, TargetType.Power)  // 15 minutes in power zone 1
            .AddRepeat("3x Build/Recovery", 3, buildStep, recoveryStep)
            .CoolDown(10, 1, TargetType.Power) // 10 minutes in power zone 1
            .Build();
    }
}