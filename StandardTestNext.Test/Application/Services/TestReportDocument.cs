namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportDocument
{
    public string RecordCode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string TestKindCode { get; set; } = string.Empty;
    public DateTimeOffset TestTime { get; set; }
    public bool IsValid { get; set; }
    public string? OwnDepartment { get; set; }
    public string? TestDepartment { get; set; }
    public string? Tester { get; set; }
    public string? Remark { get; set; }
    public TestReportMetadataDocument Metadata { get; set; } = new();
    public TestReportStatisticsDocument Statistics { get; set; } = new();
    public TestReportProductDocument? Product { get; set; }
    public TestReportProductDocument? AccompanyProduct { get; set; }
    public List<TestReportAttachmentDocument> RecordAttachments { get; } = new();
    public List<TestReportItemDocument> Items { get; } = new();
}

public sealed class TestReportMetadataDocument
{
    public string ReportFormatVersion { get; set; } = "v1";
    public string SourceBoundary { get; set; } = "StandardTestNext.Test";
    public string PayloadStrategy { get; set; } = "Phase-1 JSON payload";
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.Now;
}

public sealed class TestReportStatisticsDocument
{
    public int ItemCount { get; set; }
    public int TotalSampleCount { get; set; }
    public int KeyPointSampleCount { get; set; }
    public int ContinuousSampleCount { get; set; }
    public List<string> ItemCodes { get; } = new();
}

public sealed class TestReportProductDocument
{
    public string ProductDefinitionId { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string RatedParamsJson { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public sealed class TestReportItemDocument
{
    public string TestRecordItemId { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public bool IsValid { get; set; }
    public List<TestReportAttachmentDocument> Attachments { get; } = new();
}

public sealed class TestReportAttachmentDocument
{
    public string AttachmentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string? Remark { get; set; }
}
