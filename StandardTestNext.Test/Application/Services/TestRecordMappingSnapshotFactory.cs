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
                DisplayName = x.DisplayName,
                RecordMode = x.RecordMode ?? TestRecordSampleModes.Unknown,
                SampleCount = x.SampleCount,
                MethodCode = x.MethodCode,
                Remark = x.Remark ?? string.Empty,
                SortOrder = x.SortOrder
            })
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ItemCode, StringComparer.Ordinal)
            .ToArray();

        return new TestRecordMappingSnapshot
        {
            Partitions = partitions,
            TotalSampleCount = partitions.Sum(x => x.SampleCount),
            KeyPointSampleCount = partitions.Where(x => string.Equals(x.RecordMode, TestRecordSampleModes.KeyPointOnly, StringComparison.OrdinalIgnoreCase)).Sum(x => x.SampleCount),
            ContinuousSampleCount = partitions.Where(x => string.Equals(x.RecordMode, TestRecordSampleModes.Continuous, StringComparison.OrdinalIgnoreCase)).Sum(x => x.SampleCount)
        };
    }
}
