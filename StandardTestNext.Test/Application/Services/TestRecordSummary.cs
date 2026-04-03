namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordSummary
{
    public string RecordCode { get; init; } = string.Empty;
    public string ProductKind { get; init; } = string.Empty;
    public string? ProductCode { get; init; }
    public string? ProductModel { get; init; }
    public bool ReusedProductDefinition { get; init; }
    public string TestKindCode { get; init; } = string.Empty;
    public DateTimeOffset TestTime { get; init; }
    public int ItemCount { get; init; }
    public int RecordAttachmentCount { get; init; }
    public int ItemAttachmentBucketCount { get; init; }
    public int ReportCount { get; init; }
    public bool HasReportArtifacts { get; init; }
    public DateTimeOffset? LatestReportSavedAt { get; init; }
    public string? PrimaryReportFormat { get; init; }
    public string? PrimaryReportArtifactFileName { get; init; }
    public string? LightweightReportFormat { get; init; }
    public string? LightweightReportArtifactFileName { get; init; }
    public TestRecordMappingSnapshot Mapping { get; init; } = new();
}
