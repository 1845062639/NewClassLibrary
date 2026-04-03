using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportPersistenceService
{
    public async Task<PersistedReportArtifact> ExportWriteAndSaveAsync(
        TestReportDocument document,
        ITestReportRenderer renderer,
        ITestReportArtifactWriter artifactWriter,
        ITestReportRepository reportRepository,
        bool isPrimary,
        bool isLightweight,
        CancellationToken cancellationToken = default)
    {
        var exporter = new TestReportExportService();
        var persisted = await exporter.ExportAndWriteAsync(document, renderer, artifactWriter, cancellationToken);

        await reportRepository.SaveAsync(document, persisted.Format, persisted.Content, cancellationToken);
        await reportRepository.SaveSummaryAsync(BuildSummary(document.RecordCode, persisted, isPrimary, isLightweight), cancellationToken);

        return persisted;
    }

    public async Task SaveInlineAsync(
        TestReportDocument document,
        ITestReportRenderer renderer,
        ITestReportRepository reportRepository,
        bool isPrimary,
        bool isLightweight,
        string? savedPathPrefix = null,
        CancellationToken cancellationToken = default)
    {
        var exporter = new TestReportExportService();
        var exported = exporter.Export(document, renderer);

        await reportRepository.SaveAsync(document, exported.Format, exported.Content, cancellationToken);

        var fileName = $"{document.RecordCode}.{exported.Format}";
        var savedPath = string.IsNullOrWhiteSpace(savedPathPrefix)
            ? fileName
            : $"{savedPathPrefix.TrimEnd('/')}/{fileName}";
        var artifact = new TestReportArtifactDescriptor(
            document.RecordCode,
            exported.Format,
            fileName,
            savedPath,
            DateTimeOffset.Now);

        var persisted = new PersistedReportArtifact(exported.Format, exported.Content, artifact);
        await reportRepository.SaveSummaryAsync(BuildSummary(document.RecordCode, persisted, isPrimary, isLightweight), cancellationToken);
    }

    private static TestReportPersistenceSummary BuildSummary(
        string recordCode,
        PersistedReportArtifact persisted,
        bool isPrimary,
        bool isLightweight)
    {
        return new TestReportPersistenceSummary
        {
            RecordCode = recordCode,
            Format = persisted.Format,
            ArtifactFileName = persisted.Artifact.FileName,
            ArtifactSavedPath = persisted.Artifact.SavedPath,
            ExportedAt = persisted.Artifact.WrittenAt,
            ContentLength = persisted.Content.Length,
            IsPrimaryEntry = isPrimary,
            IsLightweightEntry = isLightweight
        };
    }
}
