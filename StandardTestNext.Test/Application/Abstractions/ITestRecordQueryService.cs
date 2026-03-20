namespace StandardTestNext.Test.Application.Services;

public interface ITestRecordQueryService
{
    Task<TestRecordDetail?> GetByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestRecordSummary>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default);
}
