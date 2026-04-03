using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportQueryService : ITestReportQueryService
{
    private readonly ITestReportRepository _reportRepository;

    public TestReportQueryService(ITestReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<IReadOnlyList<TestReportSnapshot>> ListForRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        return _reportRepository.ListForRecordCodeAsync(recordCode, cancellationToken);
    }

    public Task<IReadOnlyList<TestReportPersistenceSummary>> ListRecentSummariesAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        return _reportRepository.ListRecentSummariesAsync(take, cancellationToken);
    }
}
