using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordItemPayloadReader
{
    public static TestRecordItemPayloadSnapshot TryParse(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return new TestRecordItemPayloadSnapshot();
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            var root = document.RootElement;
            var sampleCount = root.TryGetProperty("SampleCount", out var sampleCountElement) && sampleCountElement.ValueKind == JsonValueKind.Number
                ? sampleCountElement.GetInt32()
                : GuessBusinessSampleCount(root);
            var legacySampleCount = root.TryGetProperty("LegacySampleCount", out var legacySampleCountElement) && legacySampleCountElement.ValueKind == JsonValueKind.Number
                ? legacySampleCountElement.GetInt32()
                : 0;
            var recordMode = root.TryGetProperty("RecordMode", out var recordModeElement) && recordModeElement.ValueKind == JsonValueKind.String
                ? recordModeElement.GetString()
                : GuessRecordMode(root);
            int? methodValue = root.TryGetProperty("Method", out var methodElement) && methodElement.ValueKind == JsonValueKind.Number
                ? methodElement.GetInt32()
                : root.TryGetProperty("MethodValue", out var methodValueElement) && methodValueElement.ValueKind == JsonValueKind.Number
                    ? methodValueElement.GetInt32()
                    : root.TryGetProperty("BuildProfile", out var buildProfileElement) && buildProfileElement.ValueKind == JsonValueKind.Object
                        && buildProfileElement.TryGetProperty("MethodValue", out var buildProfileMethodValueElement) && buildProfileMethodValueElement.ValueKind == JsonValueKind.Number
                        ? buildProfileMethodValueElement.GetInt32()
                        : null;
            var hasLegacyPayload = root.TryGetProperty("LegacySamples", out var legacySamplesElement)
                ? legacySamplesElement.ValueKind == JsonValueKind.Array && legacySamplesElement.GetArrayLength() > 0
                : legacySampleCount > 0;
            var hasBuildProfile = root.TryGetProperty("BuildProfile", out buildProfileElement)
                && buildProfileElement.ValueKind == JsonValueKind.Object;

            return new TestRecordItemPayloadSnapshot
            {
                SampleCount = sampleCount,
                LegacySampleCount = legacySampleCount,
                HasLegacyPayload = hasLegacyPayload,
                HasBuildProfile = hasBuildProfile,
                RecordMode = recordMode,
                MethodValue = methodValue,
                LegacyPayload = BuildLegacyPayload(root, legacySampleCount, hasLegacyPayload)
            };
        }
        catch (JsonException)
        {
            return new TestRecordItemPayloadSnapshot();
        }
    }

    public static int? TryResolveMethodValue(TestRecordItemPayloadSnapshot payload, string? methodCode)
    {
        if (payload.MethodValue.HasValue)
        {
            return payload.MethodValue.Value;
        }

        if (string.IsNullOrWhiteSpace(methodCode))
        {
            return null;
        }

        var separatorIndex = methodCode.LastIndexOf(':');
        if (separatorIndex >= 0)
        {
            var suffix = methodCode[(separatorIndex + 1)..];
            if (int.TryParse(suffix, out var parsedSuffix))
            {
                return parsedSuffix;
            }
        }

        return int.TryParse(methodCode, out var parsed)
            ? parsed
            : null;
    }

    private static TestRecordLegacyPayloadSummary BuildLegacyPayload(JsonElement root, int legacySampleCount, bool hasLegacyPayload)
    {
        if (!root.TryGetProperty("LegacySamples", out var legacySamplesElement) || legacySamplesElement.ValueKind != JsonValueKind.Array)
        {
            return new TestRecordLegacyPayloadSummary
            {
                LegacySampleCount = legacySampleCount,
                HasLegacyPayload = hasLegacyPayload
            };
        }

        var powerCurveImageCount = 0;
        var tempCurveImageCount = 0;
        var vibrationCurveImageCount = 0;
        var hasIncomingPowerMetrics = false;
        var hasWindingTemperatureMetrics = false;

        foreach (var sample in legacySamplesElement.EnumerateArray())
        {
            powerCurveImageCount += CountNonEmptyProperties(sample, "LeaveFactoryModePowerCurveImage", "TempRiseModePowerCurveImage");
            tempCurveImageCount += CountNonEmptyProperties(sample, "LeaveFactoryModeTempCurveImage", "TempRiseModeTempCurveImage");
            vibrationCurveImageCount += CountNonEmptyProperties(sample, "LeaveFactoryModeVibrationCurveImage", "LeaveFactoryModeVibrationFrequencyCurveImage", "TempRiseModeVibrationCurveImage", "TempRiseModeVibrationFrequencyCurveImage");
            hasIncomingPowerMetrics |= HasAnyNumericProperty(sample, "UabIncoming", "UbcIncoming", "UcaIncoming", "UavgIncoming", "IaIncoming", "IbIncoming", "IcIncoming", "IavgIncoming", "PIncoming", "FrequencyIncoming");
            hasWindingTemperatureMetrics |= HasAnyNumericProperty(sample, "Temp1", "Temp2", "Temp3", "Temp4", "Temp5", "Temp6", "Temp7", "Temp8");
        }

        return new TestRecordLegacyPayloadSummary
        {
            LegacySampleCount = legacySampleCount,
            HasLegacyPayload = hasLegacyPayload,
            PowerCurveImageCount = powerCurveImageCount,
            TempCurveImageCount = tempCurveImageCount,
            VibrationCurveImageCount = vibrationCurveImageCount,
            HasIncomingPowerMetrics = hasIncomingPowerMetrics,
            HasWindingTemperatureMetrics = hasWindingTemperatureMetrics
        };
    }

    private static int GuessBusinessSampleCount(JsonElement root)
    {
        if (TryGetArrayLength(root, "DataList", out var dataListCount))
        {
            return dataListCount;
        }

        if (TryGetArrayLength(root, "RawDataList", out var rawDataCount))
        {
            return rawDataCount > 0 ? rawDataCount : GuessFlatLoadASampleCount(root);
        }

        if (TryGetArrayLength(root, "ResultDataList", out var resultDataCount))
        {
            return resultDataCount > 0 ? resultDataCount : GuessFlatLoadASampleCount(root);
        }

        if (TryGetArrayLength(root, "Data1List", out var data1Count))
        {
            return data1Count;
        }

        if (TryGetArrayLength(root, "Data2List", out var data2Count))
        {
            return data2Count;
        }

        return HasAnyNumericProperty(root,
            "Ruv", "Rvw", "Rwu", "R1",
            "P0", "I0", "Pfw", "Pfe",
            "Ikn", "Pkn", "Tkn",
            "Uk", "Ik", "Pk", "Tk",
            "θw", "Δθ", "Δθn",
            "P1", "P2", "η", "Cosφ")
            ? 1
            : 0;
    }

    private static string? GuessRecordMode(JsonElement root)
    {
        if (root.TryGetProperty("DataList", out var dataList) && dataList.ValueKind == JsonValueKind.Array)
        {
            return TestRecordSampleModes.KeyPointOnly;
        }

        if (root.TryGetProperty("RawDataList", out var rawDataList) && rawDataList.ValueKind == JsonValueKind.Array)
        {
            return TestRecordSampleModes.Continuous;
        }

        if (root.TryGetProperty("Data1List", out var data1List) && data1List.ValueKind == JsonValueKind.Array)
        {
            return TestRecordSampleModes.Continuous;
        }

        if (root.TryGetProperty("Data2List", out var data2List) && data2List.ValueKind == JsonValueKind.Array)
        {
            return TestRecordSampleModes.KeyPointOnly;
        }

        return null;
    }

    private static bool TryGetArrayLength(JsonElement root, string propertyName, out int count)
    {
        if (root.TryGetProperty(propertyName, out var element) && element.ValueKind == JsonValueKind.Array)
        {
            count = element.GetArrayLength();
            return true;
        }

        count = 0;
        return false;
    }

    private static int GuessFlatLoadASampleCount(JsonElement root)
    {
        var rowLevelFields = new[] { "P1", "P2", "I1", "η", "Cosφ", "S", "Percentage" };
        if (HasAnyNumericProperty(root, rowLevelFields))
        {
            return 1;
        }

        var envelopeFields = new[] { "Un", "Pn", "R1c", "θ1c", "θa", "ΔT", "K1" };
        return HasAnyNumericProperty(root, envelopeFields) ? 1 : 0;
    }

    private static int CountNonEmptyProperties(JsonElement sample, params string[] propertyNames)
    {
        var count = 0;
        foreach (var propertyName in propertyNames)
        {
            if (sample.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                count++;
            }
        }

        return count;
    }

    private static bool HasAnyNumericProperty(JsonElement sample, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (sample.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class TestRecordItemPayloadSnapshot
{
    public int SampleCount { get; init; }
    public int LegacySampleCount { get; init; }
    public bool HasLegacyPayload { get; init; }
    public bool HasBuildProfile { get; init; }
    public TestRecordLegacyPayloadSummary LegacyPayload { get; init; } = new();
    public string? RecordMode { get; init; }
    public int? MethodValue { get; init; }
}
