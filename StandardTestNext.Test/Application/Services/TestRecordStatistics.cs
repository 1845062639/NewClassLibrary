namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordStatistics
{
    public int ItemCount { get; init; }
    public int TotalSampleCount { get; init; }
    public int KeyPointSampleCount { get; init; }
    public int ContinuousSampleCount { get; init; }
    public IReadOnlyList<string> ItemCodes { get; init; } = Array.Empty<string>();
}
