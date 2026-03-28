using System.Text.Json;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordLegacyPayloadReaderSmokeTests
{
    public static void Run()
    {
        ShouldParseLegacyPayloadSummaryFromJson();
        ShouldHandleLegacyPayloadEdgeCases();
        ShouldFormatLegacyPayloadSummaryWithMetricsFlags();
        ShouldFormatListSummaryWithPayloadFlags();
        ShouldNormalizeMotorYLegacyItemCodesFromRealStpAliases();
        ShouldParseMotorYTrialPayloadShapesFromBuilder();
    }

    private static void ShouldParseLegacyPayloadSummaryFromJson()
    {
        var payload = new
        {
            SampleCount = 3,
            LegacySampleCount = 2,
            RecordMode = "legacy-replay",
            LegacySamples = new object[]
            {
                new
                {
                    LeaveFactoryModePowerCurveImage = "power-a.png",
                    LeaveFactoryModeTempCurveImage = "temp-a.png",
                    LeaveFactoryModeVibrationCurveImage = "vibration-a.png",
                    UabIncoming = 380.5,
                    Temp1 = 88.2
                },
                new
                {
                    TempRiseModePowerCurveImage = "power-b.png",
                    TempRiseModeTempCurveImage = "temp-b.png",
                    TempRiseModeVibrationFrequencyCurveImage = "vibration-b.png"
                }
            }
        };

        var snapshot = TestRecordItemPayloadReader.TryParse(JsonSerializer.Serialize(payload));

        if (snapshot.SampleCount != 3
            || snapshot.LegacySampleCount != 2
            || !snapshot.HasLegacyPayload
            || snapshot.LegacyPayload.PowerCurveImageCount != 2
            || snapshot.LegacyPayload.TempCurveImageCount != 2
            || snapshot.LegacyPayload.VibrationCurveImageCount != 2
            || !snapshot.LegacyPayload.HasIncomingPowerMetrics
            || !snapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload reader smoke test failed.");
        }
    }

    private static void ShouldHandleLegacyPayloadEdgeCases()
    {
        var missingLegacySamplesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 4,
              "LegacySampleCount": 2,
              "RecordMode": "legacy-replay"
            }
            """);

        if (missingLegacySamplesSnapshot.SampleCount != 4
            || missingLegacySamplesSnapshot.LegacySampleCount != 2
            || !missingLegacySamplesSnapshot.HasLegacyPayload
            || missingLegacySamplesSnapshot.LegacyPayload.PowerCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.VibrationCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.HasIncomingPowerMetrics
            || missingLegacySamplesSnapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload missing-array edge case smoke test failed.");
        }

        var emptyLegacySamplesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 1,
              "LegacySampleCount": 0,
              "LegacySamples": []
            }
            """);

        if (emptyLegacySamplesSnapshot.SampleCount != 1
            || emptyLegacySamplesSnapshot.LegacySampleCount != 0
            || emptyLegacySamplesSnapshot.HasLegacyPayload
            || emptyLegacySamplesSnapshot.LegacyPayload.PowerCurveImageCount != 0
            || emptyLegacySamplesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || emptyLegacySamplesSnapshot.LegacyPayload.VibrationCurveImageCount != 0)
        {
            throw new InvalidOperationException("Legacy payload empty-array edge case smoke test failed.");
        }

        var partialCurvesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 2,
              "LegacySampleCount": 1,
              "LegacySamples": [
                {
                  "TempRiseModePowerCurveImage": "power-only.png",
                  "Temp2": 77.3
                }
              ]
            }
            """);

        if (partialCurvesSnapshot.SampleCount != 2
            || partialCurvesSnapshot.LegacySampleCount != 1
            || !partialCurvesSnapshot.HasLegacyPayload
            || partialCurvesSnapshot.LegacyPayload.PowerCurveImageCount != 1
            || partialCurvesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || partialCurvesSnapshot.LegacyPayload.VibrationCurveImageCount != 0
            || partialCurvesSnapshot.LegacyPayload.HasIncomingPowerMetrics
            || !partialCurvesSnapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload partial-curves edge case smoke test failed.");
        }
    }

    private static void ShouldFormatLegacyPayloadSummaryWithMetricsFlags()
    {
        var summary = FormatPayloadSnapshotSummary(new[]
        {
            (
                ItemCode: "Noise",
                Payload: new TestRecordItemPayloadSnapshot
                {
                    LegacySampleCount = 2,
                    LegacyPayload = new TestRecordLegacyPayloadSummary
                    {
                        LegacySampleCount = 2,
                        PowerCurveImageCount = 1,
                        TempCurveImageCount = 1,
                        VibrationCurveImageCount = 1,
                        HasIncomingPowerMetrics = true,
                        HasWindingTemperatureMetrics = false
                    }
                })
        });

        const string expected = "Noise:legacy=2:power=1:temp=1:vibration=1:incoming=Y:winding=N";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Legacy payload formatter smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }

    private static void ShouldFormatListSummaryWithPayloadFlags()
    {
        var summary = TestRecordLegacyPayloadFormatter.FormatListSummary(new[]
        {
            new TestRecordItemPartitionContract
            {
                ItemCode = "Noise",
                LegacySampleCount = 2,
                HasLegacyPayload = true
            },
            new TestRecordItemPartitionContract
            {
                ItemCode = "TempRise",
                LegacySampleCount = 0,
                HasLegacyPayload = false
            }
        });

        const string expected = "Noise:legacy=2:payload=Y, TempRise:legacy=0:payload=N";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Legacy payload list formatter smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }

    private static void ShouldNormalizeMotorYLegacyItemCodesFromRealStpAliases()
    {
        var cases = new (string Alias, string Canonical, bool IsCoreTrial)[]
        {
            ("直流电阻测定", MotorYTestMethodCodes.DcResistance, true),
            ("陪试直流电阻测定", MotorYTestMethodCodes.DcResistance, true),
            ("空载特性试验", MotorYTestMethodCodes.NoLoad, true),
            ("空载试验", MotorYTestMethodCodes.NoLoad, true),
            ("空载试验（出厂）", MotorYTestMethodCodes.NoLoad, true),
            ("空载特性测量", MotorYTestMethodCodes.NoLoad, true),
            ("空载特性完全试验", MotorYTestMethodCodes.NoLoad, true),
            ("热试验", MotorYTestMethodCodes.HeatRun, true),
            ("热试验2", MotorYTestMethodCodes.HeatRun, true),
            ("温度计法热试验", MotorYTestMethodCodes.HeatRun, true),
            ("陪试热试验", MotorYTestMethodCodes.HeatRun, true),
            ("A法负载试验", MotorYTestMethodCodes.LoadA, true),
            ("B法负载试验", MotorYTestMethodCodes.LoadB, true),
            ("堵转特性试验", MotorYTestMethodCodes.LockedRotor, true),
            ("堵转试验", MotorYTestMethodCodes.LockedRotor, true),
            ("堵转试验（出厂）", MotorYTestMethodCodes.LockedRotor, true),
            ("C法负载试验", "C法负载试验", false)
        };

        foreach (var (alias, canonical, isCoreTrial) in cases)
        {
            var normalized = MotorYLegacyItemCodeNormalizer.Normalize(alias);
            if (!string.Equals(normalized, canonical, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y alias normalize smoke test failed for '{alias}'. Expected '{canonical}', got '{normalized}'.");
            }

            if (MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(alias) != isCoreTrial)
            {
                throw new InvalidOperationException($"Motor_Y core-trial detect smoke test failed for '{alias}'.");
            }
        }
    }

    private static void ShouldParseMotorYTrialPayloadShapesFromBuilder()
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

        var baseTime = DateTimeOffset.Parse("2026-03-28T10:00:00+08:00");
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

        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.DcResistance, expectedSampleCount: 1, expectedRecordMode: null);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.NoLoad, expectedSampleCount: 3, expectedRecordMode: TestRecordSampleModes.KeyPointOnly);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.HeatRun, expectedSampleCount: 3, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LoadA, expectedSampleCount: 2, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LoadB, expectedSampleCount: 1, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LockedRotor, expectedSampleCount: 2, expectedRecordMode: TestRecordSampleModes.KeyPointOnly);
    }

    private static void AssertMotorYPayloadSnapshot(
        IReadOnlyDictionary<string, StandardTestNext.Test.Domain.Records.TestRecordItemAggregate> items,
        string itemCode,
        int expectedSampleCount,
        string? expectedRecordMode)
    {
        if (!items.TryGetValue(itemCode, out var item))
        {
            throw new InvalidOperationException($"Motor_Y builder smoke test missing item '{itemCode}'.");
        }

        var snapshot = TestRecordItemPayloadReader.TryParse(item.DataJson);
        if (snapshot.SampleCount != expectedSampleCount || !string.Equals(snapshot.RecordMode, expectedRecordMode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Motor_Y payload reader smoke test failed for '{itemCode}'. Expected sampleCount={expectedSampleCount}, recordMode={expectedRecordMode ?? "<null>"}; got sampleCount={snapshot.SampleCount}, recordMode={snapshot.RecordMode ?? "<null>"}.");
        }
    }

    private static string FormatPayloadSnapshotSummary(IEnumerable<(string ItemCode, TestRecordItemPayloadSnapshot Payload)> snapshots)
    {
        return string.Join(", ",
            snapshots.Select(x => $"{x.ItemCode}:legacy={x.Payload.LegacySampleCount}:power={x.Payload.LegacyPayload.PowerCurveImageCount}:temp={x.Payload.LegacyPayload.TempCurveImageCount}:vibration={x.Payload.LegacyPayload.VibrationCurveImageCount}:incoming={(x.Payload.LegacyPayload.HasIncomingPowerMetrics ? "Y" : "N")}:winding={(x.Payload.LegacyPayload.HasWindingTemperatureMetrics ? "Y" : "N")}"));
    }
}
