using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordDetail
{
    public required TestRecordAggregate Record { get; init; }
    public IReadOnlyList<RecordAttachment> RecordAttachments { get; init; } = Array.Empty<RecordAttachment>();
    public IReadOnlyDictionary<Guid, IReadOnlyList<RecordAttachment>> ItemAttachments { get; init; } =
        new Dictionary<Guid, IReadOnlyList<RecordAttachment>>();
    public IReadOnlyList<TestRecordItemDetail> ItemDetails { get; init; } = Array.Empty<TestRecordItemDetail>();
    public TestRecordMappingSnapshot Mapping { get; init; } = new();
    public IReadOnlyList<TestReportSnapshot> Reports { get; init; } = Array.Empty<TestReportSnapshot>();
    public IReadOnlyList<TestReportPersistenceSummary> ReportSummaries { get; init; } = Array.Empty<TestReportPersistenceSummary>();
    public bool HasReports => Reports.Count > 0;
    public bool HasReportArtifacts => Reports.Any(x => !string.IsNullOrWhiteSpace(x.ArtifactSavedPath));
    public TestReportSnapshot? LightweightReport => TestReportSelection.SelectLightweight(Reports);
    public TestReportSnapshot? PrimaryReport => TestReportSelection.SelectPrimary(Reports);
}
