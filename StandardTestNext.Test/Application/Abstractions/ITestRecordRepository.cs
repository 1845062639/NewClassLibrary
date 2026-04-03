using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestRecordRepository
{
    Task SaveAsync(TestRecordAggregate record, CancellationToken cancellationToken = default);
    Task<TestRecordAggregate?> FindByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestRecordAggregate>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default);
}
