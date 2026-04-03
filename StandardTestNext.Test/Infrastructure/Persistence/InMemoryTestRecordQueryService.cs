using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryTestRecordQueryService : ITestRecordQueryService
{
    private readonly InMemoryTestRecordRepository _repository;
    private readonly TestRecordQueryViewAssembler _viewAssembler = new();

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

        var recordAttachments = record.Attachments;
        var itemAttachments = record.Items.ToDictionary(
            x => x.TestRecordItemId,
            x => (IReadOnlyList<RecordAttachment>)x.Attachments);

        return _viewAssembler.AssembleDetail(
            record,
            recordAttachments,
            itemAttachments,
            Array.Empty<TestReportSnapshot>());
    }

    public async Task<IReadOnlyList<TestRecordSummary>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var records = await _repository.ListRecentAsync(take, cancellationToken);
        return records
            .Select(x => _viewAssembler.AssembleSummary(x, Array.Empty<TestReportSnapshot>()))
            .ToArray();
    }

    public async Task<TestRecordDetailView?> GetDetailViewByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var detail = await GetByRecordCodeAsync(recordCode, cancellationToken);
        return detail?.ToDetailView();
    }

    public async Task<IReadOnlyList<TestRecordListView>> ListRecentViewsAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var summaries = await ListRecentAsync(take, cancellationToken);
        return summaries
            .Select(x => x.ToListView())
            .ToArray();
    }
}
