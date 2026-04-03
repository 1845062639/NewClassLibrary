using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportExportService
{
    private readonly TestReportDocumentMapper _documentMapper = new();

    public TestReportDocument BuildDocument(TestRecordAggregate record, TestRecordStatistics? statistics = null)
    {
        return _documentMapper.Map(record, statistics);
    }

    public ReportExportResult Export(TestReportDocument document, ITestReportRenderer renderer)
    {
        var content = renderer.Render(document);
        return new ReportExportResult(renderer.Format, content);
    }

    public async Task<PersistedReportArtifact> ExportAndWriteAsync(
        TestReportDocument document,
        ITestReportRenderer renderer,
        ITestReportArtifactWriter artifactWriter,
        CancellationToken cancellationToken = default)
    {
        var export = Export(document, renderer);
        var artifact = await artifactWriter.WriteAsync(document.RecordCode, export.Format, export.Content, cancellationToken);
        return new PersistedReportArtifact(export.Format, export.Content, artifact);
    }
}

public sealed record ReportExportResult(string Format, string Content);
