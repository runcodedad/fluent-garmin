using Fluent.Garmin;

namespace Fluent.Garmin.Tests;

public class EnumTests
{
    [Fact]
    public void DurationType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.Time));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.Distance));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.Open));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.Calories));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.RepeatUntilStepsCmplt));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.RepeatUntilTime));
        Assert.True(Enum.IsDefined(typeof(DurationType), DurationType.RepeatUntilDistance));
    }

    [Fact]
    public void TargetType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(TargetType), TargetType.Open));
        Assert.True(Enum.IsDefined(typeof(TargetType), TargetType.HeartRate));
        Assert.True(Enum.IsDefined(typeof(TargetType), TargetType.Speed));
        Assert.True(Enum.IsDefined(typeof(TargetType), TargetType.Power));
        Assert.True(Enum.IsDefined(typeof(TargetType), TargetType.Cadence));
    }
}

public class StepDurationTests
{
    [Fact]
    public void StepDuration_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var duration = new StepDuration
        {
            Type = DurationType.Time,
            Value = 300
        };

        // Assert
        Assert.Equal(DurationType.Time, duration.Type);
        Assert.Equal(300u, duration.Value);
    }

    [Theory]
    [InlineData(DurationType.Time, 600u)] // 10 minutes
    [InlineData(DurationType.Distance, 1000u)] // 1000 meters
    [InlineData(DurationType.Calories, 200u)] // 200 calories
    public void StepDuration_ShouldSupportDifferentTypes(DurationType type, uint value)
    {
        // Arrange & Act
        var duration = new StepDuration { Type = type, Value = value };

        // Assert
        Assert.Equal(type, duration.Type);
        Assert.Equal(value, duration.Value);
    }
}

public class StepTargetTests
{
    [Fact]
    public void StepTarget_ShouldAllowZoneTargets()
    {
        // Arrange & Act
        var target = new StepTarget
        {
            Type = TargetType.HeartRate,
            Zone = 3
        };

        // Assert
        Assert.Equal(TargetType.HeartRate, target.Type);
        Assert.Equal(3u, target.Zone);
        Assert.Null(target.LowValue);
        Assert.Null(target.HighValue);
    }

    [Fact]
    public void StepTarget_ShouldAllowCustomRanges()
    {
        // Arrange & Act
        var target = new StepTarget
        {
            Type = TargetType.Power,
            LowValue = 200,
            HighValue = 250
        };

        // Assert
        Assert.Equal(TargetType.Power, target.Type);
        Assert.Equal(200u, target.LowValue);
        Assert.Equal(250u, target.HighValue);
        Assert.Null(target.Zone);
    }

    [Fact]
    public void StepTarget_ShouldAllowOpenTarget()
    {
        // Arrange & Act
        var target = new StepTarget
        {
            Type = TargetType.Open
        };

        // Assert
        Assert.Equal(TargetType.Open, target.Type);
        Assert.Null(target.Zone);
        Assert.Null(target.LowValue);
        Assert.Null(target.HighValue);
    }
}