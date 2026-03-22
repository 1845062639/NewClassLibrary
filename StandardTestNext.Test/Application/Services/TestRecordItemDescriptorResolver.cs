namespace StandardTestNext.Test.Application.Services;

public static class TestRecordItemDescriptorResolver
{
    public static string ResolveDisplayName(string itemCode, string? recordMode)
    {
        return itemCode switch
        {
            "RealtimeKeyPoints" => "Realtime Key Points",
            "RealtimeContinuous" => "Realtime Continuous Samples",
            _ => !string.IsNullOrWhiteSpace(recordMode)
                ? $"{itemCode} ({recordMode})"
                : itemCode
        };
    }

    public static int ResolveSortOrder(string itemCode, string? recordMode)
    {
        if (string.Equals(itemCode, "RealtimeKeyPoints", StringComparison.Ordinal))
        {
            return 100;
        }

        if (string.Equals(itemCode, "RealtimeContinuous", StringComparison.Ordinal))
        {
            return 200;
        }

        return string.Equals(recordMode, TestRecordSampleModes.KeyPointOnly, StringComparison.OrdinalIgnoreCase)
            ? 1000
            : string.Equals(recordMode, TestRecordSampleModes.Continuous, StringComparison.OrdinalIgnoreCase)
                ? 2000
                : 9000;
    }
}
