using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryTestRecordQueryService : ITestRecordQueryService
{
    private readonly InMemoryTestRecordRepository _repository;

    public InMemoryTestRecordQueryService(InMemoryTestRecordRepository repository)
    {
        _repository = repository;
    }

    public Task<TestRecordAggregate?> GetByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        return _repository.FindByRecordCodeAsync(recordCode, cancellationToken);
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
