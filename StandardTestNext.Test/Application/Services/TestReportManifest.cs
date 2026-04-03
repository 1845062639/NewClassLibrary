namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportManifest
{
    public string RecordCode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string TestKindCode { get; set; } = string.Empty;
    public DateTimeOffset TestTime { get; set; }
    public bool IsValid { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductModel { get; set; }
    public string? Tester { get; set; }
    public string? OwnDepartment { get; set; }
    public string? TestDepartment { get; set; }
    public string? Remark { get; set; }
    public TestReportManifestStatistics Statistics { get; set; } = new();
    public List<TestReportManifestItem> Items { get; } = new();
    public List<TestReportManifestArtifact> Artifacts { get; } = new();
}

public sealed class TestReportManifestStatistics
{
    public int ItemCount { get; set; }
    public int TotalSampleCount { get; set; }
    public int KeyPointSampleCount { get; set; }
    public int ContinuousSampleCount { get; set; }
}

public sealed class TestReportManifestItem
{
    public string ItemCode { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string RecordMode { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public bool IsValid { get; set; }
    public bool HasRemark { get; set; }
}

public sealed class TestReportManifestArtifact
{
    public string Format { get; set; } = string.Empty;
    public string ArtifactFileName { get; set; } = string.Empty;
    public string ArtifactSavedPath { get; set; } = string.Empty;
    public DateTimeOffset ExportedAt { get; set; }
    public int ContentLength { get; set; }
}
