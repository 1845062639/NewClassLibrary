namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportPersistenceSummary
{
    public string RecordCode { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string ArtifactFileName { get; set; } = string.Empty;
    public string ArtifactSavedPath { get; set; } = string.Empty;
    public DateTimeOffset ExportedAt { get; set; }
    public int ContentLength { get; set; }
    public bool IsLightweightEntry { get; set; }
    public bool IsPrimaryEntry { get; set; }
}
