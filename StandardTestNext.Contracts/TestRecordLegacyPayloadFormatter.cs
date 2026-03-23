namespace StandardTestNext.Contracts;

public static class TestRecordLegacyPayloadFormatter
{
    public static string FormatListSummary(IEnumerable<TestRecordItemPartitionContract> partitions)
    {
        return string.Join(", ",
            partitions.Select(x => $"{x.ItemCode}:legacy={x.LegacySampleCount}:payload={(x.HasLegacyPayload ? "Y" : "N")}"));
    }

    public static string FormatDetailSummary(IEnumerable<TestRecordItemDetailContract> items)
    {
        return string.Join(", ",
            items.Select(x => $"{x.ItemCode}:legacy={x.LegacySampleCount}:power={x.LegacyPayload.PowerCurveImageCount}:temp={x.LegacyPayload.TempCurveImageCount}:vibration={x.LegacyPayload.VibrationCurveImageCount}:incoming={(x.LegacyPayload.HasIncomingPowerMetrics ? "Y" : "N")}:winding={(x.LegacyPayload.HasWindingTemperatureMetrics ? "Y" : "N")}"));
    }
}
