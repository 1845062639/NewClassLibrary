namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportSnapshot
{
    public string RecordCode { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ArtifactFileName { get; set; } = string.Empty;
    public string ArtifactSavedPath { get; set; } = string.Empty;
    public DateTimeOffset SavedAt { get; set; }
}
