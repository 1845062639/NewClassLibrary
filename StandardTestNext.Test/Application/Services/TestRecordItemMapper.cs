using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemMapper
{
    public TestRecordItemMappingResult MapRealtimeSamples(IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        var items = new List<TestRecordItemAggregate>();
        var partitions = new List<TestRecordSamplePartitionSummary>();

        AppendPartition(
            samples.Where(x => x.IsRecordPoint).ToArray(),
            itemCode: "RealtimeKeyPoints",
            recordMode: "KeyPointOnly",
            remark: "Key-point samples extracted from realtime stream.",
            items,
            partitions);

        AppendPartition(
            samples.Where(x => !x.IsRecordPoint).ToArray(),
            itemCode: "RealtimeContinuous",
            recordMode: "Continuous",
            remark: "Continuous samples kept as raw JSON in phase-1.",
            items,
            partitions);

        return new TestRecordItemMappingResult
        {
            Partitions = partitions,
            Items = items
        };
    }

    private static void AppendPartition(
        IReadOnlyCollection<MotorRealtimeSampleContract> samples,
        string itemCode,
        string recordMode,
        string remark,
        ICollection<TestRecordItemAggregate> items,
        ICollection<TestRecordSamplePartitionSummary> partitions)
    {
        if (samples.Count == 0)
        {
            return;
        }

        items.Add(new TestRecordItemAggregate
        {
            ItemCode = itemCode,
            MethodCode = "MotorRealtimeSampling",
            DataJson = JsonSerializer.Serialize(new
            {
                RecordMode = recordMode,
                SampleCount = samples.Count,
                Samples = samples
            }),
            Remark = remark
        });

        partitions.Add(new TestRecordSamplePartitionSummary
        {
            ItemCode = itemCode,
            RecordMode = recordMode,
            SampleCount = samples.Count,
            MethodCode = "MotorRealtimeSampling",
            Remark = remark
        });
    }
}
