namespace StandardTestNext.Test.Application.Services;

internal static class MotorYUpstreamDependencySnapshotFactory
{
    public static MotorYUpstreamDependencySnapshot Create(
        string canonicalCode,
        IReadOnlyList<string> requiredUpstreamCanonicalCodes,
        IEnumerable<string> availableCanonicalCodes)
    {
        var required = requiredUpstreamCanonicalCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var available = availableCanonicalCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();
        var availableSet = available.ToHashSet(StringComparer.Ordinal);
        var observedRequired = required
            .Where(code => availableSet.Contains(code))
            .ToArray();
        var missing = required
            .Where(code => !availableSet.Contains(code))
            .ToArray();
        var satisfied = missing.Length == 0;
        var summary = required.Length == 0
            ? "no upstream dependencies"
            : satisfied
                ? $"upstream dependencies satisfied ({string.Join(" + ", required)}); observed {observedRequired.Length}/{required.Length} required upstream codes"
                : $"upstream dependencies missing {missing.Length}/{required.Length}: {string.Join(", ", missing)}; observed {observedRequired.Length}/{required.Length} required upstream codes";

        return new MotorYUpstreamDependencySnapshot
        {
            CanonicalCode = canonicalCode,
            ObservedUpstreamCanonicalCodeCount = observedRequired.Length,
            ObservedUpstreamCanonicalCodes = observedRequired,
            MissingUpstreamCanonicalCodes = missing,
            UpstreamDependenciesSatisfied = satisfied,
            UpstreamDependencySummary = summary
        };
    }
}

internal sealed class MotorYUpstreamDependencySnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int ObservedUpstreamCanonicalCodeCount { get; init; }
    public IReadOnlyList<string> ObservedUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public bool UpstreamDependenciesSatisfied { get; init; }
    public string UpstreamDependencySummary { get; init; } = string.Empty;
}
