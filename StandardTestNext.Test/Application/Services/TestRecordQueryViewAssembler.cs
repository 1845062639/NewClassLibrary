using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordQueryViewAssembler
{
    private readonly TestRecordMappingSnapshotFactory _mappingSnapshotFactory = new();

    public TestRecordDetail AssembleDetail(
        TestRecordAggregate record,
        IReadOnlyList<RecordAttachment> recordAttachments,
        IReadOnlyDictionary<Guid, IReadOnlyList<RecordAttachment>> itemAttachments,
        IReadOnlyList<TestReportSnapshot> reportSnapshots)
    {
        var itemDetails = record.Items
            .Select(item =>
            {
                itemAttachments.TryGetValue(item.TestRecordItemId, out var attachments);
                return BuildItemDetail(item, attachments ?? Array.Empty<RecordAttachment>());
            })
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ItemCode, StringComparer.Ordinal)
            .ToArray();

        var reportSummaries = reportSnapshots
            .Select(x => new TestReportPersistenceSummary
            {
                RecordCode = x.RecordCode,
                Format = x.Format,
                ExportedAt = x.SavedAt,
                ContentLength = x.Content.Length,
                ArtifactFileName = x.ArtifactFileName,
                ArtifactSavedPath = x.ArtifactSavedPath,
                IsLightweightEntry = x.IsLightweightEntry,
                IsPrimaryEntry = x.IsPrimaryEntry
            })
            .OrderByDescending(x => x.ExportedAt)
            .ToArray();

        return new TestRecordDetail
        {
            Record = record,
            RecordAttachments = recordAttachments,
            ItemAttachments = itemAttachments,
            ItemDetails = itemDetails,
            Mapping = _mappingSnapshotFactory.Build(itemDetails),
            Reports = reportSnapshots,
            ReportSummaries = reportSummaries
        };
    }

    public TestRecordSummary AssembleSummary(TestRecordAggregate record, IReadOnlyList<TestReportSnapshot> reports)
    {
        var latestReport = reports
            .OrderByDescending(x => x.SavedAt)
            .FirstOrDefault();
        var primaryReport = TestReportSelection.SelectPrimary(reports);
        var lightweightReport = TestReportSelection.SelectLightweight(reports);

        var itemDetails = record.Items
            .Select(item => BuildItemDetail(item, item.Attachments))
            .ToArray();

        return new TestRecordSummary
        {
            RecordCode = record.RecordCode,
            ProductKind = record.ProductKind,
            ProductCode = record.TestProduct?.Code,
            ProductModel = record.TestProduct?.Model,
            ReusedProductDefinition = record.TestProduct is not null,
            TestKindCode = record.TestKindCode,
            TestTime = record.TestTime,
            ItemCount = record.Items.Count,
            RecordAttachmentCount = record.Attachments.Count,
            ItemAttachmentBucketCount = record.Items.Count(item => item.Attachments.Count > 0),
            ReportCount = reports.Count,
            HasReportArtifacts = reports.Any(x => !string.IsNullOrWhiteSpace(x.ArtifactSavedPath)),
            LatestReportSavedAt = latestReport?.SavedAt,
            PrimaryReportFormat = primaryReport?.Format,
            PrimaryReportArtifactFileName = primaryReport?.ArtifactFileName,
            LightweightReportFormat = lightweightReport?.Format,
            LightweightReportArtifactFileName = lightweightReport?.ArtifactFileName,
            Mapping = _mappingSnapshotFactory.Build(itemDetails)
        };
    }

    private static TestRecordItemDetail BuildItemDetail(TestRecordItemAggregate item, IReadOnlyList<RecordAttachment> attachments)
    {
        var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
        return new TestRecordItemDetail
        {
            TestRecordItemId = item.TestRecordItemId,
            ItemCode = item.ItemCode,
            DisplayName = TestRecordItemDescriptorResolver.ResolveDisplayName(item.ItemCode, payload.RecordMode),
            MethodCode = item.MethodCode,
            IsValid = item.IsValid,
            Remark = item.Remark,
            HasRemark = !string.IsNullOrWhiteSpace(item.Remark),
            AttachmentCount = attachments.Count,
            SampleCount = payload.SampleCount,
            LegacySampleCount = payload.LegacySampleCount,
            HasLegacyPayload = payload.HasLegacyPayload,
            LegacyPayload = payload.LegacyPayload,
            RecordMode = payload.RecordMode,
            SortOrder = TestRecordItemDescriptorResolver.ResolveSortOrder(item.ItemCode, payload.RecordMode)
        };
    }
}
