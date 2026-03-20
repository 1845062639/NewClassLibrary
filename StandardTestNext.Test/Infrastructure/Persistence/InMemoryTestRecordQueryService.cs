using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryTestRecordQueryService : ITestRecordQueryService
{
    private readonly InMemoryTestRecordRepository _repository;

    public InMemoryTestRecordQueryService(InMemoryTestRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<TestRecordDetail?> GetByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var record = await _repository.FindByRecordCodeAsync(recordCode, cancellationToken);
        if (record is null)
        {
            return null;
        }

        return new TestRecordDetail
        {
            Record = record,
            RecordAttachments = record.Attachments,
            ItemAttachments = record.Items.ToDictionary(
                x => x.TestRecordItemId,
                x => (IReadOnlyList<Domain.Records.RecordAttachment>)x.Attachments),
            ItemDetails = record.Items
                .Select(x => new TestRecordItemDetail
                {
                    TestRecordItemId = x.TestRecordItemId,
                    ItemCode = x.ItemCode,
                    MethodCode = x.MethodCode,
                    IsValid = x.IsValid,
                    Remark = x.Remark,
                    HasRemark = !string.IsNullOrWhiteSpace(x.Remark),
                    AttachmentCount = x.Attachments.Count,
                    SampleCount = 0,
                    RecordMode = null
                })
                .ToArray(),
            Reports = Array.Empty<TestReportSnapshot>(),
            ReportSummaries = Array.Empty<TestReportPersistenceSummary>()
        };
    }

    public async Task<IReadOnlyList<TestRecordSummary>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var records = await _repository.ListRecentAsync(take, cancellationToken);
        return records
            .Select(x => new TestRecordSummary
            {
                RecordCode = x.RecordCode,
                ProductKind = x.ProductKind,
                TestKindCode = x.TestKindCode,
                TestTime = x.TestTime,
                ItemCount = x.Items.Count,
                RecordAttachmentCount = x.Attachments.Count
            })
            .ToArray();
    }
}
