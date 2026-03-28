namespace StandardTestNext.Test.Application.Services;

internal static class MotorYStructuredListCoverageEvaluator
{
    public static MotorYStructuredListCoverageSnapshot Evaluate(
        IReadOnlyList<string>? requiredItems,
        IReadOnlyList<string>? actualItems,
        string summaryLabel)
    {
        var required = (requiredItems ?? Array.Empty<string>())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var actual = (actualItems ?? Array.Empty<string>())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var covered = required
            .Where(item => actual.Contains(item, StringComparer.Ordinal))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var missing = required
            .Where(item => !covered.Contains(item, StringComparer.Ordinal))
            .ToArray();
        var ratio = required.Length == 0
            ? 1d
            : Math.Round((double)covered.Length / required.Length, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYStructuredListCoverageSnapshot
        {
            RequiredCount = required.Length,
            CoveredCount = covered.Length,
            MissingCount = missing.Length,
            CoveredItems = covered,
            MissingItems = missing,
            CoverageRatio = ratio,
            CoveragePercentagePoints = percentagePoints,
            Summary = $"{summaryLabel} covered {covered.Length}/{required.Length} ({percentagePoints}pp); missing: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}"
        };
    }
}
