namespace StandardTestNext.Contracts;

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
    public int RecordAttachmentCount { get; init; }
    public int ItemAttachmentBucketCount { get; init; }
    public int ReportCount { get; init; }
    public bool HasReportArtifacts { get; init; }
    public bool ReusedProductDefinition { get; init; }
    public string? PrimaryReportFormat { get; init; }
    public string? PrimaryReportArtifactFileName { get; init; }
    public string? LightweightReportFormat { get; init; }
    public string? LightweightReportArtifactFileName { get; init; }
    public IReadOnlyList<TestRecordItemPartitionContract> ItemPartitions { get; init; } = Array.Empty<TestRecordItemPartitionContract>();
}

public sealed class TestRecordDetailContract
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
    public IReadOnlyList<TestRecordItemDetailContract> ItemDetails { get; init; } = Array.Empty<TestRecordItemDetailContract>();
    public IReadOnlyList<TestReportSummaryContract> ReportSummaries { get; init; } = Array.Empty<TestReportSummaryContract>();
}

public sealed class TestRecordItemDetailContract
{
    public string ItemCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public string RecordMode { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public int AttachmentCount { get; init; }
    public bool IsValid { get; init; }
    public bool HasRemark { get; init; }
    public string? Remark { get; init; }
}

public sealed class TestRecordItemPartitionContract
{
    public string ItemCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public string RecordMode { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public bool HasRemark { get; init; }
    public string? Remark { get; init; }
}

public sealed class TestReportSummaryContract
{
    public string RecordCode { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public DateTimeOffset ExportedAt { get; init; }
    public int ContentLength { get; init; }
    public string? ArtifactFileName { get; init; }
    public string? ArtifactSavedPath { get; init; }
    public bool IsLightweightEntry { get; init; }
    public bool IsPrimaryEntry { get; init; }
}
