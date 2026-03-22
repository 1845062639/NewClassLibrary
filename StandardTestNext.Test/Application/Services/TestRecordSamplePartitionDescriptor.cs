namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordSamplePartitionDescriptor
{
    public required string ItemCode { get; init; }
    public required string DisplayName { get; init; }
    public required string RecordMode { get; init; }
    public required string MethodCode { get; init; }
    public required string Remark { get; init; }
    public int SortOrder { get; init; }

    public Func<StandardTestNext.Contracts.MotorRealtimeSampleContract, bool> Predicate { get; init; } = _ => false;
}
