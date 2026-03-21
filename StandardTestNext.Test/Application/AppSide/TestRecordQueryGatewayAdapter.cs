using StandardTestNext.App.Application;
using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.AppSide;

public sealed class TestRecordQueryGatewayAdapter : ITestRecordQueryGateway
{
    private readonly TestRecordQueryFacade _queryFacade;

    public TestRecordQueryGatewayAdapter(TestRecordQueryFacade queryFacade)
    {
        _queryFacade = queryFacade;
    }

    public async Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var views = await _queryFacade.ListRecentForAppAsync(take, cancellationToken);
        return views.Select(MapListItem).ToArray();
    }

    public async Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var view = await _queryFacade.GetDetailForAppAsync(recordCode, cancellationToken);
        return view is null ? null : MapDetail(view);
    }

    private static TestRecordListItemContract MapListItem(TestRecordListView view)
    {
        return new TestRecordListItemContract
        {
            RecordCode = view.RecordCode,
            ProductKind = view.ProductKind,
            ProductDisplayName = view.ProductDisplayName,
            TestKindCode = view.TestKindCode,
            TestTime = view.TestTime,
            ItemCount = view.ItemCount,
            SampleCount = view.SampleCount,
            KeyPointSampleCount = view.KeyPointSampleCount,
            ContinuousSampleCount = view.ContinuousSampleCount,
            ReportCount = view.ReportCount,
            HasReportArtifacts = view.HasReportArtifacts,
            ReusedProductDefinition = view.ReusedProductDefinition,
            PrimaryReportFormat = view.PrimaryReportFormat,
            PrimaryReportArtifactFileName = view.PrimaryReportArtifactFileName,
            LightweightReportFormat = view.LightweightReportFormat,
            LightweightReportArtifactFileName = view.LightweightReportArtifactFileName
        };
    }

    private static TestRecordDetailContract MapDetail(TestRecordDetailView view)
    {
        return new TestRecordDetailContract
        {
            RecordCode = view.RecordCode,
            ProductKind = view.ProductKind,
            ProductDisplayName = view.ProductDisplayName,
            TestKindCode = view.TestKindCode,
            TestTime = view.TestTime,
            RecordAttachmentCount = view.RecordAttachmentCount,
            ItemAttachmentBucketCount = view.ItemAttachmentBucketCount,
            ItemCount = view.ItemCount,
            SampleCount = view.SampleCount,
            KeyPointSampleCount = view.KeyPointSampleCount,
            ContinuousSampleCount = view.ContinuousSampleCount,
            HasReports = view.HasReports,
            HasReportArtifacts = view.HasReportArtifacts,
            PrimaryReportFormat = view.PrimaryReportFormat,
            PrimaryReportArtifactFileName = view.PrimaryReportArtifactFileName,
            LightweightReportFormat = view.LightweightReportFormat,
            LightweightReportArtifactFileName = view.LightweightReportArtifactFileName
        };
    }
}
