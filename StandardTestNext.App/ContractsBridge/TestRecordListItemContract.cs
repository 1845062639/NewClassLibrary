namespace StandardTestNext.App.ContractsBridge;

public sealed class TestRecordListItemContract
{
    public string RecordCode { get; init; } = string.Empty;
    public string ProductKind { get; init; } = string.Empty;
    public string ProductDisplayName { get; init; } = string.Empty;
    public string TestKindCode { get; init; } = string.Empty;
    public DateTimeOffset TestTime { get; init; }
    public int ItemCount { get; init; }
    public int SampleCount { get; init; }
    public int KeyPointSampleCount { get; init; }
    public int ContinuousSampleCount { get; init; }
    public int ReportCount { get; init; }
    public bool HasReportArtifacts { get; init; }
    public bool ReusedProductDefinition { get; init; }
    public string? PrimaryReportFormat { get; init; }
    public string? PrimaryReportArtifactFileName { get; init; }
    public string? LightweightReportFormat { get; init; }
    public string? LightweightReportArtifactFileName { get; init; }
}
