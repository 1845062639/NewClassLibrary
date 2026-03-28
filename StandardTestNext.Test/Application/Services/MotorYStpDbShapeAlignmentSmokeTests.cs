using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYStpDbShapeAlignmentSmokeTests
{
    public static void Run()
    {
        var builder = new MotorYTrialRecordBuilder();
        var rated = new MotorRatedParamsContract
        {
            ProductKind = "Motor_Y",
            Model = "Y2-315M-4",
            RatedPower = 132,
            RatedCurrent = 240,
            RatedVoltage = 380,
            RatedSpeed = 1480,
            RatedFrequency = 50,
            Pole = 4,
            PolePairs = 2,
            Connection = "Δ"
        };

        var baseTime = DateTimeOffset.Parse("2026-03-28T11:00:00+08:00");
        var samples = new[]
        {
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime,
                ProductKind = "Motor_Y",
                VoltageAverage = 381.2,
                CurrentAverage = 52.6,
                Power = 24.8,
                Frequency = 50,
                Speed = 1492,
                Torque = 118.2,
                IsRecordPoint = true
            },
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime.AddSeconds(10),
                ProductKind = "Motor_Y",
                VoltageAverage = 379.8,
                CurrentAverage = 66.4,
                Power = 31.2,
                Frequency = 50,
                Speed = 1476,
                Torque = 136.5,
                IsRecordPoint = false
            },
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime.AddSeconds(20),
                ProductKind = "Motor_Y",
                VoltageAverage = 382.5,
                CurrentAverage = 74.1,
                Power = 36.7,
                Frequency = 50,
                Speed = 1468,
                Torque = 149.3,
                IsRecordPoint = true
            }
        };

        var items = builder.BuildTrialItems(rated, samples).ToDictionary(x => x.ItemCode, StringComparer.Ordinal);

        AssertDcResistance(items[MotorYTestMethodCodes.DcResistance]);
        AssertNoLoad(items[MotorYTestMethodCodes.NoLoad]);
        AssertHeatRun(items[MotorYTestMethodCodes.HeatRun]);
        AssertLoadA(items[MotorYTestMethodCodes.LoadA]);
        AssertLoadB(items[MotorYTestMethodCodes.LoadB]);
        AssertLockedRotor(items[MotorYTestMethodCodes.LockedRotor]);
    }

    private static void AssertDcResistance(TestRecordItemAggregate item)
    {
        var shape = System.Text.Json.JsonDocument.Parse(item.DataJson).RootElement;
        if (!shape.TryGetProperty("Ruv", out _)
            || !shape.TryGetProperty("Rvw", out _)
            || !shape.TryGetProperty("Rwu", out _)
            || !shape.TryGetProperty("R1c", out _)
            || !shape.TryGetProperty("θ1c", out _)
            || !shape.TryGetProperty("Connection", out _))
        {
            throw new InvalidOperationException($"Motor_Y 直流电阻 builder 未覆盖 stp.db 关键字段组。payload={item.DataJson}");
        }
    }

    private static void AssertNoLoad(TestRecordItemAggregate item)
    {
        var shape = MotorYNoLoadLegacyShape.FromJson(item.DataJson);
        if (shape is null
            || shape.DataList.Count == 0
            || shape.CoefficientOfPfe.Length == 0
            || shape.Un <= 0
            || shape.R1c <= 0)
        {
            throw new InvalidOperationException("Motor_Y 空载 builder 未形成可消费的旧结构。");
        }

        if (!item.DataJson.Contains("\"U0DivideUnIsEquesToOne_I0\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"U0DivideUnIsEquesToOne_P0\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"RConverseType\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y 空载 builder 未覆盖 stp.db 关键字段组。");
        }
    }

    private static void AssertHeatRun(TestRecordItemAggregate item)
    {
        var shape = MotorYThermalLegacyShape.FromJson(item.DataJson);
        if (shape is null
            || shape.Data1List.Count == 0
            || shape.Data2List.Count == 0
            || shape.Rc <= 0
            || shape.Rn <= 0)
        {
            throw new InvalidOperationException("Motor_Y 热试验 builder 未形成可消费的旧结构。");
        }

        using var jsonDocument = System.Text.Json.JsonDocument.Parse(item.DataJson);
        var root = jsonDocument.RootElement;
        if (!root.TryGetProperty("θbChanelSelect", out _)
            || !root.TryGetProperty("θ1ChanelSelect", out _)
            || !root.TryGetProperty("IsManual", out _)
            || !root.TryGetProperty("Δθn", out _))
        {
            throw new InvalidOperationException($"Motor_Y 热试验 builder 未覆盖 next-gen 扩展字段组。payload={item.DataJson}");
        }
    }

    private static void AssertLoadA(TestRecordItemAggregate item)
    {
        var shape = MotorYLoadALegacyShape.FromJson(item.DataJson);
        if (shape is null
            || shape.RawDataList.Count == 0
            || shape.ResultDataList.Count == 0
            || shape.Un <= 0
            || shape.Pn <= 0)
        {
            throw new InvalidOperationException("Motor_Y A法负载 builder 未形成可消费的旧结构。");
        }

        if (!item.DataJson.Contains("\"K2\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"CoefficientOfPfe\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"CorrectionType\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y A法负载 builder 未覆盖 stp.db 关键字段组。");
        }
    }

    private static void AssertLoadB(TestRecordItemAggregate item)
    {
        var shape = MotorYLoadBLegacyShape.FromJson(item.DataJson);
        if (shape is null
            || shape.RawDataList.Count == 0
            || shape.ResultDataList.Count == 0
            || shape.Pcu1 <= 0
            || shape.Pcu2 <= 0)
        {
            throw new InvalidOperationException("Motor_Y B法负载 builder 未形成可消费的旧结构。");
        }

        using var jsonDocument = System.Text.Json.JsonDocument.Parse(item.DataJson);
        var root = jsonDocument.RootElement;
        if (!root.TryGetProperty("TorqueCorrection", out _)
            || !root.TryGetProperty("θ1tChanelSelect", out _)
            || !root.TryGetProperty("θaChanelSelect", out _))
        {
            throw new InvalidOperationException($"Motor_Y B法负载 builder 未覆盖 stp.db/旧结构关键字段组。payload={item.DataJson}");
        }
    }

    private static void AssertLockedRotor(TestRecordItemAggregate item)
    {
        var shape = MotorYLockRotorLegacyShape.FromJson(item.DataJson);
        if (shape is null
            || shape.DataList.Count == 0
            || shape.In <= 0
            || shape.Tn <= 0)
        {
            throw new InvalidOperationException("Motor_Y 堵转 builder 未形成可消费的旧结构。");
        }

        if (!item.DataJson.Contains("\"IknDivideIn\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"TknDivideTn\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"TorqueCalType\"", StringComparison.Ordinal)
            || !item.DataJson.Contains("\"RCalType\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y 堵转 builder 未覆盖 stp.db 关键字段组。");
        }
    }
}
