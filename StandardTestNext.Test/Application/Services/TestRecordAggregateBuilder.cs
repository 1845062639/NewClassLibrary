using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordAggregateBuilder
{
    private readonly TestRecordItemMapper _itemMapper = new();

    public TestRecordBuildResult BuildDemoRecord(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples,
        ProductDefinition? productDefinition = null)
    {
        var record = new TestRecordAggregate
        {
            RecordCode = $"TR-{DateTimeOffset.Now:yyyyMMddHHmmss}",
            SerialNumber = $"SN-{DateTimeOffset.Now:yyyyMMddHHmmss}",
            ProductKind = rated.ProductKind,
            TestKindCode = "MotorRoutine",
            OwnDepartment = "TestCenter",
            TestDepartment = "ElectricalLab",
            Tester = "system-demo",
            Remark = "Demo aggregate built from next-gen contracts.",
            TestTime = DateTimeOffset.Now,
            TestProduct = productDefinition
        };

        var mappingResult = _itemMapper.MapRealtimeSamples(samples);
        foreach (var item in mappingResult.Items)
        {
            record.Items.Add(item);
        }

        var statistics = new TestRecordStatistics
        {
            ItemCount = mappingResult.Items.Count,
            TotalSampleCount = mappingResult.Partitions.Sum(x => x.SampleCount),
            KeyPointSampleCount = mappingResult.Partitions.Where(x => x.RecordMode == TestRecordSampleModes.KeyPointOnly).Sum(x => x.SampleCount),
            ContinuousSampleCount = mappingResult.Partitions.Where(x => x.RecordMode == TestRecordSampleModes.Continuous).Sum(x => x.SampleCount),
            ItemCodes = mappingResult.Items.Select(x => x.ItemCode).ToArray()
        };

        return new TestRecordBuildResult
        {
            Record = record,
            Mapping = mappingResult,
            Statistics = statistics
        };
    }
}
