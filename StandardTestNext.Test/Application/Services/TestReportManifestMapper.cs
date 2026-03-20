using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportManifestMapper
{

    public TestReportManifest Map(
        TestRecordAggregate record,
        TestRecordStatistics? statistics,
        IReadOnlyCollection<TestReportPersistenceSummary>? artifacts = null)
    {
        var manifest = new TestReportManifest
        {
            RecordCode = record.RecordCode,
            SerialNumber = record.SerialNumber,
            ProductKind = record.ProductKind,
            TestKindCode = record.TestKindCode,
            TestTime = record.TestTime,
            IsValid = record.IsValid,
            ProductCode = record.TestProduct?.Code,
            ProductModel = record.TestProduct?.Model,
            Tester = record.Tester,
            OwnDepartment = record.OwnDepartment,
            TestDepartment = record.TestDepartment,
            Remark = record.Remark,
            Statistics = new TestReportManifestStatistics
            {
                ItemCount = statistics?.ItemCount ?? record.Items.Count,
                TotalSampleCount = statistics?.TotalSampleCount ?? 0,
                KeyPointSampleCount = statistics?.KeyPointSampleCount ?? 0,
                ContinuousSampleCount = statistics?.ContinuousSampleCount ?? 0
            }
        };

        foreach (var item in record.Items)
        {
            var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
            manifest.Items.Add(new TestReportManifestItem
            {
                ItemCode = item.ItemCode,
                MethodCode = item.MethodCode,
                RecordMode = payload.RecordMode ?? string.Empty,
                SampleCount = payload.SampleCount,
                IsValid = item.IsValid,
                HasRemark = !string.IsNullOrWhiteSpace(item.Remark)
            });
        }

        if (artifacts is not null)
        {
            foreach (var artifact in artifacts.OrderBy(x => x.ExportedAt))
            {
                manifest.Artifacts.Add(new TestReportManifestArtifact
                {
                    Format = artifact.Format,
                    ArtifactFileName = artifact.ArtifactFileName,
                    ArtifactSavedPath = artifact.ArtifactSavedPath,
                    ExportedAt = artifact.ExportedAt,
                    ContentLength = artifact.ContentLength
                });
            }
        }

        return manifest;
    }
}
