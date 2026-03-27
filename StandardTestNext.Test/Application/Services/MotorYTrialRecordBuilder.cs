using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYTrialRecordBuilder
{
    public IReadOnlyList<TestRecordItemAggregate> BuildTrialItems(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        var keyPointSamples = samples
            .Where(sample => sample.IsRecordPoint)
            .OrderBy(sample => sample.SampleTime)
            .ToArray();

        var continuousSamples = samples
            .Where(sample => !sample.IsRecordPoint)
            .OrderBy(sample => sample.SampleTime)
            .ToArray();

        var items = new List<TestRecordItemAggregate>
        {
            BuildDcResistanceItem(rated),
            BuildNoLoadItem(rated, keyPointSamples, continuousSamples),
            BuildHeatRunItem(rated, samples),
            BuildLoadAItem(rated, keyPointSamples),
            BuildLoadBItem(rated, continuousSamples),
            BuildLockedRotorItem(rated, keyPointSamples)
        };

        return items;
    }

    private static TestRecordItemAggregate BuildDcResistanceItem(MotorRatedParamsContract rated)
    {
        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.DcResistance",
            MethodCode = MotorYTestMethodCodes.DcResistance,
            DataJson = JsonSerializer.Serialize(new
            {
                Phase = "A-B/B-C/C-A",
                ResistanceOhm = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                AmbientTemperature = 26.5,
                Instrument = "JYR-40E (demo)",
                Source = "phase-1 trial skeleton"
            }),
            Remark = "Motor_Y 直流电阻试验骨架数据。"
        };
    }

    private static TestRecordItemAggregate BuildNoLoadItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> keyPointSamples,
        IReadOnlyList<MotorRealtimeSampleContract> continuousSamples)
    {
        var allSamples = keyPointSamples.Concat(continuousSamples).ToArray();
        var avgCurrent = allSamples.Length == 0 ? 0 : Math.Round(allSamples.Average(x => x.CurrentAverage), 3);
        var avgPower = allSamples.Length == 0 ? 0 : Math.Round(allSamples.Average(x => x.Power), 3);
        var avgSpeed = allSamples.Length == 0 ? rated.RatedSpeed : Math.Round(allSamples.Average(x => x.Speed), 1);

        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.NoLoad",
            MethodCode = MotorYTestMethodCodes.NoLoad,
            DataJson = JsonSerializer.Serialize(new
            {
                RatedVoltage = rated.RatedVoltage,
                RatedFrequency = rated.RatedFrequency,
                KeyPointCount = keyPointSamples.Count,
                ContinuousCount = continuousSamples.Count,
                AverageCurrent = avgCurrent,
                AveragePower = avgPower,
                AverageSpeed = avgSpeed,
                Source = "mapped from realtime samples"
            }),
            Remark = "Motor_Y 空载试验最小闭环项。"
        };
    }

    private static TestRecordItemAggregate BuildHeatRunItem(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        var latest = samples.OrderByDescending(x => x.SampleTime).FirstOrDefault();
        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.HeatRun",
            MethodCode = MotorYTestMethodCodes.HeatRun,
            DataJson = JsonSerializer.Serialize(new
            {
                Stage = "trial-skeleton",
                RatedCurrent = rated.RatedCurrent,
                EstimatedWindingTemperature = latest is null ? 0 : Math.Round(latest.CurrentAverage * 2.1 + 35, 2),
                EstimatedAmbientTemperature = 26.5,
                Source = "phase-1 trial skeleton"
            }),
            Remark = "Motor_Y 热试验占位项，后续替换为真实温升链路。"
        };
    }

    private static TestRecordItemAggregate BuildLoadAItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> keyPointSamples)
    {
        var sample = keyPointSamples.LastOrDefault();
        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LoadA",
            MethodCode = MotorYTestMethodCodes.LoadA,
            DataJson = JsonSerializer.Serialize(new
            {
                LoadMode = "A",
                RatedPower = rated.RatedPower,
                MeasuredTorque = sample?.Torque ?? 0,
                MeasuredPower = sample?.Power ?? 0,
                Source = "key point snapshot"
            }),
            Remark = "Motor_Y A 法负载试验占位项。"
        };
    }

    private static TestRecordItemAggregate BuildLoadBItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> continuousSamples)
    {
        var sample = continuousSamples.LastOrDefault();
        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LoadB",
            MethodCode = MotorYTestMethodCodes.LoadB,
            DataJson = JsonSerializer.Serialize(new
            {
                LoadMode = "B",
                RatedPower = rated.RatedPower,
                MeasuredTorque = sample?.Torque ?? 0,
                MeasuredPower = sample?.Power ?? 0,
                ContinuousCount = continuousSamples.Count,
                Source = "continuous snapshot"
            }),
            Remark = "Motor_Y B 法负载试验占位项。"
        };
    }

    private static TestRecordItemAggregate BuildLockedRotorItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> keyPointSamples)
    {
        var sample = keyPointSamples.FirstOrDefault();
        return new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LockedRotor",
            MethodCode = MotorYTestMethodCodes.LockedRotor,
            DataJson = JsonSerializer.Serialize(new
            {
                RatedVoltage = rated.RatedVoltage,
                LockedCurrent = sample?.CurrentAverage ?? 0,
                LockedTorque = sample?.Torque ?? 0,
                Source = "trial skeleton derived from earliest key point"
            }),
            Remark = "Motor_Y 堵转试验占位项。"
        };
    }
}
