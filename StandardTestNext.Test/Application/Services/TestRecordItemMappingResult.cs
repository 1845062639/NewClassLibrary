using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemMappingResult
{
    public IReadOnlyList<TestRecordItemAggregate> Items { get; init; } = Array.Empty<TestRecordItemAggregate>();
    public IReadOnlyList<TestRecordSamplePartitionSummary> Partitions { get; init; } = Array.Empty<TestRecordSamplePartitionSummary>();
}

public sealed class TestRecordSamplePartitionSummary
{
    public string ItemCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string RecordMode { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public int LegacySampleCount { get; init; }
    public bool HasLegacyPayload { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public string Remark { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
