namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordMappingSnapshotFactory
{
    public TestRecordMappingSnapshot Build(IReadOnlyCollection<TestRecordItemDetail> itemDetails)
    {
        var partitions = itemDetails
            .Where(x => x.SampleCount > 0)
            .Select(x => new TestRecordSamplePartitionSummary
            {
                ItemCode = x.ItemCode,
                RecordMode = x.RecordMode ?? "Unknown",
                SampleCount = x.SampleCount,
                MethodCode = x.MethodCode,
                Remark = x.Remark ?? string.Empty
            })
            .ToArray();

        return new TestRecordMappingSnapshot
        {
            Partitions = partitions,
            TotalSampleCount = partitions.Sum(x => x.SampleCount),
            KeyPointSampleCount = partitions.Where(x => string.Equals(x.RecordMode, "KeyPointOnly", StringComparison.OrdinalIgnoreCase)).Sum(x => x.SampleCount),
            ContinuousSampleCount = partitions.Where(x => string.Equals(x.RecordMode, "Continuous", StringComparison.OrdinalIgnoreCase)).Sum(x => x.SampleCount)
        };
    }
}
