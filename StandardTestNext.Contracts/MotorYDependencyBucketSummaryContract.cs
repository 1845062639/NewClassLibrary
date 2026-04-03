namespace StandardTestNext.Contracts;

public sealed class MotorYDependencyBucketSummaryContract
{
    public string BucketKey { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int RequiredCount { get; init; }
    public int CoveredCount { get; init; }
    public int MissingCount { get; init; }
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public IReadOnlyList<string> RequiredItems { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredItems { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingItems { get; init; } = Array.Empty<string>();
    public string Summary { get; init; } = string.Empty;
}
