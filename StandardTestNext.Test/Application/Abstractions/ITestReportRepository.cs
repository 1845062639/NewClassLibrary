using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestReportRepository
{
    Task SaveAsync(TestReportDocument document, string format, string content, CancellationToken cancellationToken = default);
    Task SaveSummaryAsync(TestReportPersistenceSummary summary, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestReportSnapshot>> ListForRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestReportPersistenceSummary>> ListRecentSummariesAsync(int take = 10, CancellationToken cancellationToken = default);
}
