using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.App.Application;

public sealed class TestRecordQueryGatewayStub : ITestRecordQueryGateway
{
    public Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TestRecordListItemContract> items =
        [
            new TestRecordListItemContract
            {
                RecordCode = "PENDING-BRIDGE",
                ProductKind = "Motor_Y",
                ProductDisplayName = "Motor_Y / bridge-pending",
                TestKindCode = "Routine",
                TestTime = DateTimeOffset.Now,
                ItemCount = 0,
                SampleCount = 0,
                KeyPointSampleCount = 0,
                ContinuousSampleCount = 0,
                ReportCount = 0,
                HasReportArtifacts = false,
                ReusedProductDefinition = false
            }
        ];

        return Task.FromResult(items);
    }

    public Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        TestRecordDetailContract detail = new()
        {
            RecordCode = recordCode,
            ProductKind = "Motor_Y",
            ProductDisplayName = "Motor_Y / bridge-pending",
            TestKindCode = "Routine",
            TestTime = DateTimeOffset.Now,
            RecordAttachmentCount = 0,
            ItemAttachmentBucketCount = 0,
            ItemCount = 0,
            SampleCount = 0,
            KeyPointSampleCount = 0,
            ContinuousSampleCount = 0,
            HasReports = false,
            HasReportArtifacts = false
        };

        return Task.FromResult<TestRecordDetailContract?>(detail);
    }
}
