namespace StandardTestNext.Test.Application.Services;

public sealed record PersistedReportArtifact(
    string Format,
    string Content,
    TestReportArtifactDescriptor Artifact);
