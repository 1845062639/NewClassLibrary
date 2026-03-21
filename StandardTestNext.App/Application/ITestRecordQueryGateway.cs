using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.App.Application;

public interface ITestRecordQueryGateway
{
    Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default);
}
