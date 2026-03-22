using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordSamplePartitioner
{
    private readonly ITestRecordSamplePartitionStrategy _strategy;

    public TestRecordSamplePartitioner()
        : this(new DefaultMotorRealtimeSamplePartitionStrategy())
    {
    }

    public TestRecordSamplePartitioner(ITestRecordSamplePartitionStrategy strategy)
    {
        _strategy = strategy;
    }

    public IReadOnlyList<TestRecordSamplePartition> Partition(IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        return _strategy.Partition(samples);
    }
}

public sealed class TestRecordSamplePartition
{
    public required TestRecordSamplePartitionDescriptor Descriptor { get; init; }
    public IReadOnlyList<MotorRealtimeSampleContract> Samples { get; init; } = Array.Empty<MotorRealtimeSampleContract>();
}
