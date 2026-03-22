using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemMapper
{
    private readonly TestRecordSamplePartitioner _partitioner = new();

    public TestRecordItemMappingResult MapRealtimeSamples(
        IReadOnlyCollection<MotorRealtimeSampleContract> samples,
        IReadOnlyCollection<LegacyMotorRealtimeEnvelopeContract>? legacySamples = null)
    {
        var partitions = _partitioner.Partition(samples);
        if (partitions.Count == 0)
        {
            return new TestRecordItemMappingResult();
        }

        var legacySamplesBySampleTime = legacySamples?
            .GroupBy(sample => sample.SampleTime)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<LegacyMotorRealtimeEnvelopeContract>)group.ToArray());

        var mappedPartitions = partitions
            .Select(partition => MapPartition(partition, legacySamplesBySampleTime))
            .ToArray();

        return new TestRecordItemMappingResult
        {
            Partitions = mappedPartitions.Select(x => x.Summary).ToArray(),
            Items = mappedPartitions.Select(x => x.Item).ToArray()
        };
    }

    private static MappedPartitionResult MapPartition(
        TestRecordSamplePartition partition,
        IReadOnlyDictionary<DateTimeOffset, IReadOnlyList<LegacyMotorRealtimeEnvelopeContract>>? legacySamplesBySampleTime)
    {
        var matchedLegacySamples = partition.Samples
            .SelectMany(sample => TryGetLegacySamples(sample.SampleTime, legacySamplesBySampleTime))
            .ToArray();

        return new MappedPartitionResult(
            new TestRecordSamplePartitionSummary
            {
                ItemCode = partition.Descriptor.ItemCode,
                DisplayName = partition.Descriptor.DisplayName,
                RecordMode = partition.Descriptor.RecordMode,
                SampleCount = partition.Samples.Count,
                LegacySampleCount = matchedLegacySamples.Length,
                HasLegacyPayload = matchedLegacySamples.Length > 0,
                MethodCode = partition.Descriptor.MethodCode,
                Remark = partition.Descriptor.Remark,
                SortOrder = partition.Descriptor.SortOrder
            },
            new TestRecordItemAggregate
            {
                ItemCode = partition.Descriptor.ItemCode,
                MethodCode = partition.Descriptor.MethodCode,
                DataJson = JsonSerializer.Serialize(new
                {
                    RecordMode = partition.Descriptor.RecordMode,
                    SampleCount = partition.Samples.Count,
                    Samples = partition.Samples,
                    LegacySampleCount = matchedLegacySamples.Length,
                    LegacySamples = matchedLegacySamples
                }),
                Remark = partition.Descriptor.Remark
            });
    }

    private static IReadOnlyList<LegacyMotorRealtimeEnvelopeContract> TryGetLegacySamples(
        DateTimeOffset sampleTime,
        IReadOnlyDictionary<DateTimeOffset, IReadOnlyList<LegacyMotorRealtimeEnvelopeContract>>? legacySamplesBySampleTime)
    {
        if (legacySamplesBySampleTime is null || !legacySamplesBySampleTime.TryGetValue(sampleTime, out var matchedLegacySamples))
        {
            return Array.Empty<LegacyMotorRealtimeEnvelopeContract>();
        }

        return matchedLegacySamples;
    }

    private sealed record MappedPartitionResult(
        TestRecordSamplePartitionSummary Summary,
        TestRecordItemAggregate Item);
}
