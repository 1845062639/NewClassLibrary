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
            ReportCount = summary.ReportCount,
            HasReportArtifacts = summary.HasReportArtifacts,
            ReusedProductDefinition = summary.ReusedProductDefinition,
            PrimaryReportFormat = summary.PrimaryReportFormat,
            PrimaryReportArtifactFileName = summary.PrimaryReportArtifactFileName,
            LightweightReportFormat = summary.LightweightReportFormat,
            LightweightReportArtifactFileName = summary.LightweightReportArtifactFileName
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
