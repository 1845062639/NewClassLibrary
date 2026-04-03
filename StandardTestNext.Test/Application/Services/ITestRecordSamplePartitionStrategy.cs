using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public interface ITestRecordSamplePartitionStrategy
{
    IReadOnlyList<TestRecordSamplePartition> Partition(IReadOnlyCollection<MotorRealtimeSampleContract> samples);
}

public sealed class DefaultMotorRealtimeSamplePartitionStrategy : ITestRecordSamplePartitionStrategy
{
    private static readonly IReadOnlyList<TestRecordSamplePartitionDescriptor> DefaultDescriptors =
    [
        new TestRecordSamplePartitionDescriptor
        {
            ItemCode = "RealtimeKeyPoints",
            DisplayName = "Realtime Key Points",
            RecordMode = TestRecordSampleModes.KeyPointOnly,
            MethodCode = "MotorRealtimeSampling",
            Remark = "Key-point samples extracted from realtime stream.",
            SortOrder = 100,
            Predicate = sample => sample.IsRecordPoint
        },
        new TestRecordSamplePartitionDescriptor
        {
            ItemCode = "RealtimeContinuous",
            DisplayName = "Realtime Continuous Samples",
            RecordMode = TestRecordSampleModes.Continuous,
            MethodCode = "MotorRealtimeSampling",
            Remark = "Continuous samples kept as raw JSON in phase-1.",
            SortOrder = 200,
            Predicate = sample => !sample.IsRecordPoint
        }
    ];

    public IReadOnlyList<TestRecordSamplePartition> Partition(IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        if (samples.Count == 0)
        {
            return Array.Empty<TestRecordSamplePartition>();
        }

        var partitions = new List<TestRecordSamplePartition>();
        foreach (var descriptor in DefaultDescriptors)
        {
            var matchedSamples = samples.Where(descriptor.Predicate).ToArray();
            if (matchedSamples.Length == 0)
            {
                continue;
            }

            partitions.Add(new TestRecordSamplePartition
            {
                Descriptor = descriptor,
                Samples = matchedSamples
            });
        }

        return partitions
            .OrderBy(x => x.Descriptor.SortOrder)
            .ThenBy(x => x.Descriptor.ItemCode, StringComparer.Ordinal)
            .ToArray();
    }
}
