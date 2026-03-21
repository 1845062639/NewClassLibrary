using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordQueryService : ITestRecordQueryService
{
    private readonly ITestRecordRepository _recordRepository;
    private readonly IRecordAttachmentRepository _attachmentRepository;
    private readonly ITestReportRepository _reportRepository;
    private readonly TestRecordQueryViewAssembler _viewAssembler = new();

    public TestRecordQueryService(
        ITestRecordRepository recordRepository,
        IRecordAttachmentRepository attachmentRepository,
        ITestReportRepository reportRepository)
    {
        _recordRepository = recordRepository;
        _attachmentRepository = attachmentRepository;
        _reportRepository = reportRepository;
    }

    public async Task<TestRecordDetail?> GetByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var record = await _recordRepository.FindByRecordCodeAsync(recordCode, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var recordAttachments = await _attachmentRepository.ListForRecordAsync(record.TestRecordId, cancellationToken);
        var reportSnapshots = await _reportRepository.ListForRecordCodeAsync(record.RecordCode, cancellationToken);
        var itemAttachments = new Dictionary<Guid, IReadOnlyList<RecordAttachment>>();

        foreach (var item in record.Items)
        {
            var attachments = await _attachmentRepository.ListForRecordItemAsync(item.TestRecordItemId, cancellationToken);
            itemAttachments[item.TestRecordItemId] = attachments;
        }

        return _viewAssembler.AssembleDetail(record, recordAttachments, itemAttachments, reportSnapshots);
    }

    public async Task<IReadOnlyList<TestRecordSummary>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var records = await _recordRepository.ListRecentAsync(take, cancellationToken);
        var summaries = new List<TestRecordSummary>(records.Count);

        foreach (var record in records)
        {
            var reports = await _reportRepository.ListForRecordCodeAsync(record.RecordCode, cancellationToken);
            summaries.Add(_viewAssembler.AssembleSummary(record, reports));
        }

        return summaries;
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
