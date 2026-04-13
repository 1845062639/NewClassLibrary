using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordAggregateBuilder
{
    private readonly TestRecordItemMapper _itemMapper = new();
    private readonly MotorYTrialRecordBuilder _motorYTrialRecordBuilder = new();

    public TestRecordBuildResult BuildDemoRecord(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples,
        IReadOnlyCollection<LegacyMotorRealtimeEnvelopeContract>? legacySamples = null,
        ProductDefinition? productDefinition = null,
        int noLoadRConverseType = 0)
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

        foreach (var trialItem in _motorYTrialRecordBuilder.BuildTrialItems(rated, samples, noLoadRConverseType))
        {
            record.Items.Add(trialItem);
        }

        var mappingResult = _itemMapper.MapRealtimeSamples(samples, legacySamples);
        foreach (var item in mappingResult.Items)
        {
            record.Items.Add(item);
        }

        var statistics = new TestRecordStatistics
        {
            ItemCount = record.Items.Count,
            TotalSampleCount = mappingResult.Partitions.Sum(x => x.SampleCount),
            KeyPointSampleCount = mappingResult.Partitions.Where(x => x.RecordMode == TestRecordSampleModes.KeyPointOnly).Sum(x => x.SampleCount),
            ContinuousSampleCount = mappingResult.Partitions.Where(x => x.RecordMode == TestRecordSampleModes.Continuous).Sum(x => x.SampleCount),
            ItemCodes = record.Items.Select(x => x.ItemCode).ToArray()
        };

        return new TestRecordBuildResult
        {
            Record = record,
            Mapping = mappingResult,
            Statistics = statistics
        };
    }
}
