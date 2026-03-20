using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordItemPayloadReader
{
    public static (int SampleCount, string? RecordMode) TryParse(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return (0, null);
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            var root = document.RootElement;
            var sampleCount = root.TryGetProperty("SampleCount", out var sampleCountElement) && sampleCountElement.ValueKind == JsonValueKind.Number
                ? sampleCountElement.GetInt32()
                : 0;
            var recordMode = root.TryGetProperty("RecordMode", out var recordModeElement) && recordModeElement.ValueKind == JsonValueKind.String
                ? recordModeElement.GetString()
                : null;
            return (sampleCount, recordMode);
        }
        catch (JsonException)
        {
            return (0, null);
        }
    }
}
