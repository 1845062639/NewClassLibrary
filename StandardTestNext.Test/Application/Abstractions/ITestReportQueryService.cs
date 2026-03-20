using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestReportQueryService
{
    Task<IReadOnlyList<TestReportSnapshot>> ListForRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestReportPersistenceSummary>> ListRecentSummariesAsync(int take = 10, CancellationToken cancellationToken = default);
}
