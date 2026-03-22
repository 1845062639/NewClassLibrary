using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemMapper
{
    private readonly TestRecordSamplePartitioner _partitioner = new();

    public TestRecordItemMappingResult MapRealtimeSamples(IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        var partitions = _partitioner.Partition(samples);
        if (partitions.Count == 0)
        {
            return new TestRecordItemMappingResult();
        }

        return new TestRecordItemMappingResult
        {
            Partitions = partitions.Select(MapPartitionSummary).ToArray(),
            Items = partitions.Select(MapItem).ToArray()
        };
    }

    private static TestRecordItemAggregate MapItem(TestRecordSamplePartition partition)
    {
        return new TestRecordItemAggregate
        {
            ItemCode = partition.Descriptor.ItemCode,
            MethodCode = partition.Descriptor.MethodCode,
            DataJson = JsonSerializer.Serialize(new
            {
                RecordMode = partition.Descriptor.RecordMode,
                SampleCount = partition.Samples.Count,
                Samples = partition.Samples
            }),
            Remark = partition.Descriptor.Remark
        };
    }

    private static TestRecordSamplePartitionSummary MapPartitionSummary(TestRecordSamplePartition partition)
    {
        return new TestRecordSamplePartitionSummary
        {
            ItemCode = partition.Descriptor.ItemCode,
            DisplayName = partition.Descriptor.DisplayName,
            RecordMode = partition.Descriptor.RecordMode,
            SampleCount = partition.Samples.Count,
            MethodCode = partition.Descriptor.MethodCode,
            Remark = partition.Descriptor.Remark,
            SortOrder = partition.Descriptor.SortOrder
        };
    }
}
