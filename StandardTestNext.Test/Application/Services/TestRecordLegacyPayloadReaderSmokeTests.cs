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

    private static string FormatPayloadSnapshotSummary(IEnumerable<(string ItemCode, TestRecordItemPayloadSnapshot Payload)> snapshots)
    {
        return string.Join(", ",
            snapshots.Select(x => $"{x.ItemCode}:legacy={x.Payload.LegacySampleCount}:power={x.Payload.LegacyPayload.PowerCurveImageCount}:temp={x.Payload.LegacyPayload.TempCurveImageCount}:vibration={x.Payload.LegacyPayload.VibrationCurveImageCount}:incoming={(x.Payload.LegacyPayload.HasIncomingPowerMetrics ? "Y" : "N")}:winding={(x.Payload.LegacyPayload.HasWindingTemperatureMetrics ? "Y" : "N")}"));
    }
}
