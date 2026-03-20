namespace StandardTestNext.Test.Application.Services;

public sealed record TestReportArtifactDescriptor(
    string RecordCode,
    string Format,
    string FileName,
    string SavedPath,
    DateTimeOffset WrittenAt);
