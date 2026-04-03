using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryTestRecordRepository : ITestRecordRepository
{
    private readonly List<TestRecordAggregate> _records = new();

    public Task SaveAsync(TestRecordAggregate record, CancellationToken cancellationToken = default)
    {
        var existingIndex = _records.FindIndex(x => x.TestRecordId == record.TestRecordId || string.Equals(x.RecordCode, record.RecordCode, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
        {
            _records[existingIndex] = record;
        }
        else
        {
            _records.Add(record);
        }

        return Task.CompletedTask;
    }

    public Task<TestRecordAggregate?> FindByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var record = _records.FirstOrDefault(x => string.Equals(x.RecordCode, recordCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<TestRecordAggregate>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TestRecordAggregate> records = _records
            .OrderByDescending(x => x.TestTime)
            .Take(take)
            .ToArray();
        return Task.FromResult(records);
    }

    public IReadOnlyList<TestRecordAggregate> GetAll() => _records;
}
