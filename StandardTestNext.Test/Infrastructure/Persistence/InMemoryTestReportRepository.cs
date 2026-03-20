using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryTestReportRepository : ITestReportRepository
{
    private readonly List<StoredTestReport> _reports = new();
    private readonly List<TestReportPersistenceSummary> _summaries = new();

    public Task SaveAsync(TestReportDocument document, string format, string content, CancellationToken cancellationToken = default)
    {
        _reports.Add(new StoredTestReport(document, format, content, DateTimeOffset.Now));
        return Task.CompletedTask;
    }

    public Task SaveSummaryAsync(TestReportPersistenceSummary summary, CancellationToken cancellationToken = default)
    {
        _summaries.RemoveAll(x => x.RecordCode == summary.RecordCode && x.Format == summary.Format);
        _summaries.Add(summary);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TestReportSnapshot>> ListForRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var summariesByFormat = _summaries
            .Where(x => x.RecordCode == recordCode)
            .GroupBy(x => x.Format, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.ExportedAt).First(),
                StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<TestReportSnapshot> snapshots = _reports
            .Where(x => x.Document.RecordCode == recordCode)
            .OrderByDescending(x => x.SavedAt)
            .Select(x =>
            {
                summariesByFormat.TryGetValue(x.Format, out var summary);
                return new TestReportSnapshot
                {
                    RecordCode = x.Document.RecordCode,
                    Format = x.Format,
                    Content = x.Content,
                    ArtifactFileName = summary?.ArtifactFileName ?? string.Empty,
                    ArtifactSavedPath = summary?.ArtifactSavedPath ?? string.Empty,
                    SavedAt = x.SavedAt,
                    IsLightweightEntry = summary?.IsLightweightEntry ?? false,
                    IsPrimaryEntry = summary?.IsPrimaryEntry ?? false
                };
            })
            .ToList();
        return Task.FromResult(snapshots);
    }

    public Task<IReadOnlyList<TestReportPersistenceSummary>> ListRecentSummariesAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TestReportPersistenceSummary> summaries = _summaries
            .OrderByDescending(x => x.ExportedAt)
            .Take(take)
            .ToList();
        return Task.FromResult(summaries);
    }

    public IReadOnlyList<StoredTestReport> GetAll() => _reports;

    public IReadOnlyList<TestReportPersistenceSummary> GetSummaries() => _summaries;
}

public sealed record StoredTestReport(
    TestReportDocument Document,
    string Format,
    string Content,
    DateTimeOffset SavedAt);
