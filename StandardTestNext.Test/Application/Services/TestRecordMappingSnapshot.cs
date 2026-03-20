namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordMappingSnapshot
{
    public IReadOnlyList<TestRecordSamplePartitionSummary> Partitions { get; init; } = Array.Empty<TestRecordSamplePartitionSummary>();
    public int TotalSampleCount { get; init; }
    public int KeyPointSampleCount { get; init; }
    public int ContinuousSampleCount { get; init; }
}
