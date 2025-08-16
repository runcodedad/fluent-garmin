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
            workoutMesg.SetNumValidSteps((ushort)workout.Steps.Count);
            encodeDemo.Write(workoutMesg);

            // Create workout steps
            for (ushort i = 0; i < workout.Steps.Count; i++)
            {
                var step = workout.Steps[i];
                var workoutStep = new WorkoutStepMesg();
                
                workoutStep.SetMessageIndex(i);
                workoutStep.SetWktStepName(step.Name ?? $"Step {i + 1}");
                workoutStep.SetIntensity(step.Intensity);

                // Set duration
                SetStepDuration(workoutStep, step.Duration);

                // Set target
                SetStepTarget(workoutStep, step.Target);

                encodeDemo.Write(workoutStep);
            }

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
                    // TODO: Need to investigate correct SetTargetValue signature for HeartRate
                    // workoutStep.SetTargetValue(0, target.LowValue.Value);
                    // workoutStep.SetTargetValue(1, target.HighValue.Value);
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
                    // TODO: Need to investigate correct SetTargetValue signature for Speed
                    // workoutStep.SetTargetValue(0, target.LowValue.Value);
                    // workoutStep.SetTargetValue(1, target.HighValue.Value);
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
                    // TODO: Need to investigate correct SetTargetValue signature for Power
                    // workoutStep.SetTargetValue(0, target.LowValue.Value);
                    // workoutStep.SetTargetValue(1, target.HighValue.Value);
                }
                break;

            case TargetType.Cadence:
                workoutStep.SetTargetType(WktStepTarget.Cadence);
                if (target.LowValue.HasValue && target.HighValue.HasValue)
                {
                    // TODO: Need to investigate correct SetTargetValue signature for Cadence
                    // workoutStep.SetTargetValue(0, target.LowValue.Value);
                    // workoutStep.SetTargetValue(1, target.HighValue.Value);
                }
                break;

            case TargetType.Open:
            default:
                workoutStep.SetTargetType(WktStepTarget.Open);
                break;
        }
    }

    // Example workout models
    public static WorkoutModel CreateIntervalWorkoutModel()
    {
        return new WorkoutModel
        {
            Name = "5x400m Track Intervals",
            Sport = Sport.Running,
            Steps = new List<WorkoutStep>
            {
                new WorkoutStep
                {
                    Name = "Warm Up",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 600 }, // 10 min
                    Intensity = Intensity.Warmup,
                    Target = new StepTarget { Type = TargetType.HeartRate, Zone = 1 }
                },
                new WorkoutStep
                {
                    Name = "Interval 1",
                    Duration = new StepDuration { Type = DurationType.Distance, Value = 400 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget { Type = TargetType.Speed, Zone = 4 }
                },
                new WorkoutStep
                {
                    Name = "Recovery 1",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 120 }, // 2 min
                    Intensity = Intensity.Rest,
                    Target = new StepTarget { Type = TargetType.Open }
                },
                new WorkoutStep
                {
                    Name = "Interval 2",
                    Duration = new StepDuration { Type = DurationType.Distance, Value = 400 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget { Type = TargetType.Speed, Zone = 4 }
                },
                new WorkoutStep
                {
                    Name = "Recovery 2",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 120 },
                    Intensity = Intensity.Rest,
                    Target = new StepTarget { Type = TargetType.Open }
                },
                new WorkoutStep
                {
                    Name = "Cool Down",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 600 },
                    Intensity = Intensity.Cooldown,
                    Target = new StepTarget { Type = TargetType.HeartRate, Zone = 1 }
                }
            }
        };
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
        return new WorkoutModel
        {
            Name = "Power Intervals",
            Sport = Sport.Cycling,
            Steps = new List<WorkoutStep>
            {
                new WorkoutStep
                {
                    Name = "Warm Up",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 900 },
                    Intensity = Intensity.Warmup,
                    Target = new StepTarget { Type = TargetType.Power, Zone = 1 }
                },
                new WorkoutStep
                {
                    Name = "Build 1",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 300 }, // 5 min
                    Intensity = Intensity.Active,
                    Target = new StepTarget { Type = TargetType.Power, Zone = 3 }
                },
                new WorkoutStep
                {
                    Name = "Recovery",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 180 }, // 3 min
                    Intensity = Intensity.Rest,
                    Target = new StepTarget { Type = TargetType.Power, Zone = 1 }
                },
                new WorkoutStep
                {
                    Name = "Build 2",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 300 },
                    Intensity = Intensity.Active,
                    Target = new StepTarget { Type = TargetType.Power, Zone = 4 }
                },
                new WorkoutStep
                {
                    Name = "Cool Down",
                    Duration = new StepDuration { Type = DurationType.Time, Value = 600 },
                    Intensity = Intensity.Cooldown,
                    Target = new StepTarget { Type = TargetType.Power, Zone = 1 }
                }
            }
        };
    }
}