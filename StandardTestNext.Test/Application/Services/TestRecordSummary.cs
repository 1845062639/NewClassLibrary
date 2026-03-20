namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordSummary
{
    public string RecordCode { get; init; } = string.Empty;
    public string ProductKind { get; init; } = string.Empty;
    public string TestKindCode { get; init; } = string.Empty;
    public DateTimeOffset TestTime { get; init; }
    public int ItemCount { get; init; }
    public int RecordAttachmentCount { get; init; }
}
