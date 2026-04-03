namespace StandardTestNext.Test.Application.Services;

internal static class MotorYUpstreamDependencySnapshotFactory
{
    public static MotorYUpstreamDependencySnapshot Create(
        string canonicalCode,
        IReadOnlyList<string> requiredUpstreamCanonicalCodes,
        IEnumerable<string> availableCanonicalCodes)
    {
        return Create(canonicalCode, requiredUpstreamCanonicalCodes, availableCanonicalCodes, null);
    }

    public static MotorYUpstreamDependencySnapshot Create(
        string canonicalCode,
        IReadOnlyList<string> requiredUpstreamCanonicalCodes,
        IEnumerable<string> availableCanonicalCodes,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? observedLegacyCodesByCanonicalCode)
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
        var observedLegacyCodes = BuildObservedLegacyCodes(required, observedLegacyCodesByCanonicalCode);
        var summary = required.Length == 0
            ? "no upstream dependencies"
            : satisfied
                ? $"upstream dependencies satisfied ({string.Join(" + ", required)}); observed {observedRequired.Length}/{required.Length} required upstream codes"
                : $"upstream dependencies missing {missing.Length}/{required.Length}: {string.Join(", ", missing)}; observed {observedRequired.Length}/{required.Length} required upstream codes";

        var legacySummary = observedLegacyCodes.Count == 0
            ? "no legacy upstream aliases observed"
            : $"observed legacy upstream aliases: {string.Join(", ", observedLegacyCodes.Select(x => $"{x.Key}=[{string.Join("|", x.Value)}]"))}";

        return new MotorYUpstreamDependencySnapshot
        {
            CanonicalCode = canonicalCode,
            ObservedUpstreamCanonicalCodeCount = observedRequired.Length,
            ObservedUpstreamCanonicalCodes = observedRequired,
            ObservedUpstreamLegacyCodes = observedLegacyCodes,
            MissingUpstreamCanonicalCodes = missing,
            UpstreamDependenciesSatisfied = satisfied,
            UpstreamDependencySummary = $"{summary}; {legacySummary}"
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildObservedLegacyCodes(
        IReadOnlyList<string> requiredUpstreamCanonicalCodes,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? observedLegacyCodesByCanonicalCode)
    {
        if (requiredUpstreamCanonicalCodes.Count == 0 || observedLegacyCodesByCanonicalCode is null || observedLegacyCodesByCanonicalCode.Count == 0)
        {
            return new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        }

        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var canonicalCode in requiredUpstreamCanonicalCodes)
        {
            if (!observedLegacyCodesByCanonicalCode.TryGetValue(canonicalCode, out var observedLegacyCodes))
            {
                continue;
            }

            var ordered = observedLegacyCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToArray();
            if (ordered.Length > 0)
            {
                result[canonicalCode] = ordered;
            }
        }

        return result;
    }
}

internal sealed class MotorYUpstreamDependencySnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int ObservedUpstreamCanonicalCodeCount { get; init; }
    public IReadOnlyList<string> ObservedUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ObservedUpstreamLegacyCodes { get; init; } = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public bool UpstreamDependenciesSatisfied { get; init; }
    public string UpstreamDependencySummary { get; init; } = string.Empty;
}
