namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordLegacyPayloadSummary
{
    public int LegacySampleCount { get; init; }
    public bool HasLegacyPayload { get; init; }
    public int PowerCurveImageCount { get; init; }
    public int TempCurveImageCount { get; init; }
    public int VibrationCurveImageCount { get; init; }
    public bool HasIncomingPowerMetrics { get; init; }
    public bool HasWindingTemperatureMetrics { get; init; }
}
