using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordQueryFacade
{
    private readonly ITestRecordQueryService _queryService;

    public TestRecordQueryFacade(ITestRecordQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<IReadOnlyList<TestRecordListView>> ListRecentForAppAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        return _queryService.ListRecentViewsAsync(take, cancellationToken);
    }

    public Task<TestRecordDetailView?> GetDetailForAppAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        return _queryService.GetDetailViewByRecordCodeAsync(recordCode, cancellationToken);
    }
}
