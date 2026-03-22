namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemDetail
{
    public Guid TestRecordItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string MethodCode { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public string? Remark { get; init; }
    public bool HasRemark { get; init; }
    public int AttachmentCount { get; init; }
    public int SampleCount { get; init; }
    public int LegacySampleCount { get; init; }
    public bool HasLegacyPayload { get; init; }
    public TestRecordLegacyPayloadSummary LegacyPayload { get; init; } = new();
    public string? RecordMode { get; init; }
    public int SortOrder { get; init; }
}
