namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordDetailView
{
    public string RecordCode { get; init; } = string.Empty;
    public string ProductKind { get; init; } = string.Empty;
    public string ProductDisplayName { get; init; } = string.Empty;
    public string TestKindCode { get; init; } = string.Empty;
    public DateTimeOffset TestTime { get; init; }
    public int RecordAttachmentCount { get; init; }
    public int ItemAttachmentBucketCount { get; init; }
    public int ItemCount { get; init; }
    public int SampleCount { get; init; }
    public int KeyPointSampleCount { get; init; }
    public int ContinuousSampleCount { get; init; }
    public bool HasReports { get; init; }
    public bool HasReportArtifacts { get; init; }
    public string? PrimaryReportFormat { get; init; }
    public string? PrimaryReportArtifactFileName { get; init; }
    public string? LightweightReportFormat { get; init; }
    public string? LightweightReportArtifactFileName { get; init; }
    public IReadOnlyList<TestRecordItemDetail> ItemDetails { get; init; } = Array.Empty<TestRecordItemDetail>();
    public IReadOnlyList<MotorYMethodDecisionSnapshot> MotorYMethodDecisions { get; init; } = Array.Empty<MotorYMethodDecisionSnapshot>();
    public IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> MotorYMethodAdaptationPlans { get; init; } = Array.Empty<MotorYMethodAdaptationPlanSnapshot>();
    public IReadOnlyList<TestReportPersistenceSummary> ReportSummaries { get; init; } = Array.Empty<TestReportPersistenceSummary>();
}
