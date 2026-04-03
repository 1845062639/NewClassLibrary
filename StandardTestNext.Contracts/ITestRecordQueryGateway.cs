namespace StandardTestNext.Contracts;

public interface ITestRecordQueryGateway
{
    Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default);
}
