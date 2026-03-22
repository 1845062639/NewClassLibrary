namespace StandardTestNext.Contracts;

public static class TestRecordQueryGatewayFactory
{
    public static ITestRecordQueryGateway Create(Func<ITestRecordQueryGateway?>? resolver = null)
    {
        var resolvedGateway = resolver?.Invoke();
        return resolvedGateway ?? new NullTestRecordQueryGateway();
    }

    private sealed class NullTestRecordQueryGateway : ITestRecordQueryGateway
    {
        public Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<TestRecordListItemContract> items = Array.Empty<TestRecordListItemContract>();
            return Task.FromResult(items);
        }

        public Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TestRecordDetailContract?>(null);
        }
    }
}
