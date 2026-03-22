using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordViewMapper
{
    public static TestRecordListView ToListView(this TestRecordSummary summary)
    {
        return new TestRecordListView
        {
            RecordCode = summary.RecordCode,
            ProductKind = summary.ProductKind,
            ProductDisplayName = BuildProductDisplayName(summary.ProductModel, summary.ProductCode, summary.ProductKind),
            TestKindCode = summary.TestKindCode,
            TestTime = summary.TestTime,
            ItemCount = summary.ItemCount,
            SampleCount = summary.Mapping.TotalSampleCount,
            KeyPointSampleCount = summary.Mapping.KeyPointSampleCount,
            ContinuousSampleCount = summary.Mapping.ContinuousSampleCount,
            RecordAttachmentCount = summary.RecordAttachmentCount,
            ItemAttachmentBucketCount = summary.ItemAttachmentBucketCount,
            ReportCount = summary.ReportCount,
            HasReportArtifacts = summary.HasReportArtifacts,
            ReusedProductDefinition = summary.ReusedProductDefinition,
            PrimaryReportFormat = summary.PrimaryReportFormat,
            PrimaryReportArtifactFileName = summary.PrimaryReportArtifactFileName,
            LightweightReportFormat = summary.LightweightReportFormat,
            LightweightReportArtifactFileName = summary.LightweightReportArtifactFileName,
            ItemPartitions = summary.Mapping.Partitions
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ItemCode, StringComparer.Ordinal)
                .Select(x => new TestRecordItemPartitionContract
                {
                    ItemCode = x.ItemCode,
                    DisplayName = x.DisplayName,
                    SortOrder = x.SortOrder,
                    MethodCode = x.MethodCode,
                    RecordMode = x.RecordMode,
                    SampleCount = x.SampleCount,
                    HasRemark = !string.IsNullOrWhiteSpace(x.Remark),
                    Remark = x.Remark
                })
                .ToArray()
        };
    }

    public static TestRecordDetailView ToDetailView(this TestRecordDetail detail)
    {
        return new TestRecordDetailView
        {
            RecordCode = detail.Record.RecordCode,
            ProductKind = detail.Record.ProductKind,
            ProductDisplayName = BuildProductDisplayName(detail.Record.TestProduct?.Model, detail.Record.TestProduct?.Code, detail.Record.ProductKind),
            TestKindCode = detail.Record.TestKindCode,
            TestTime = detail.Record.TestTime,
            RecordAttachmentCount = detail.RecordAttachments.Count,
            ItemAttachmentBucketCount = detail.ItemAttachments.Count,
            ItemCount = detail.ItemDetails.Count,
            SampleCount = detail.Mapping.TotalSampleCount,
            KeyPointSampleCount = detail.Mapping.KeyPointSampleCount,
            ContinuousSampleCount = detail.Mapping.ContinuousSampleCount,
            HasReports = detail.HasReports,
            HasReportArtifacts = detail.HasReportArtifacts,
            PrimaryReportFormat = detail.PrimaryReport?.Format,
            PrimaryReportArtifactFileName = detail.PrimaryReport?.ArtifactFileName,
            LightweightReportFormat = detail.LightweightReport?.Format,
            LightweightReportArtifactFileName = detail.LightweightReport?.ArtifactFileName,
            ItemDetails = detail.ItemDetails,
            ReportSummaries = detail.ReportSummaries
        };
    }

    private static string BuildProductDisplayName(string? model, string? code, string productKind)
    {
        if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(code))
        {
            return $"{model} ({code})";
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            return model;
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return productKind;
    }
}
