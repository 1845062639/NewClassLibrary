namespace StandardTestNext.Test.Application.Services;

internal static class MotorYLegacyCodeSelectionFactory
{
    public static MotorYLegacyCodeSelectionSnapshot Build(
        string canonicalCode,
        IReadOnlyList<MotorYLegacyCodeDistributionSnapshot>? legacyCodeDistributions,
        string unavailableSummary)
    {
        var distributions = (legacyCodeDistributions ?? Array.Empty<MotorYLegacyCodeDistributionSnapshot>())
            .Where(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();

        if (distributions.Length == 0)
        {
            return new MotorYLegacyCodeSelectionSnapshot
            {
                CanonicalCode = canonicalCode,
                Summary = unavailableSummary,
                ConflictSummary = $"legacy code variants unavailable for {canonicalCode}"
            };
        }

        var recommended = distributions[0];
        var totalCount = distributions.Sum(x => x.Count);
        var runnerUp = distributions.Skip(1).FirstOrDefault();
        var dominantLeadCount = Math.Max(0, recommended.Count - (runnerUp?.Count ?? 0));
        var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((recommended.Share - (runnerUp?.Share ?? 0d)) * 100d, MidpointRounding.AwayFromZero));
        var conflictSummary = distributions.Length <= 1
            ? $"legacy code variants stable for {canonicalCode}: only '{recommended.LegacyCode}' observed"
            : $"legacy code variants for {canonicalCode}: '{recommended.LegacyCode}' leads runner-up '{runnerUp?.LegacyCode}' by {dominantLeadCount} rows ({dominantLeadPercentagePoints}pp) across {distributions.Length} aliases";

        return new MotorYLegacyCodeSelectionSnapshot
        {
            CanonicalCode = canonicalCode,
            RecommendedLegacyCode = recommended.LegacyCode,
            DominantLegacyCode = recommended.LegacyCode,
            RecommendedLegacyCodeCount = recommended.Count,
            RecommendedLegacyCodeShare = recommended.Share,
            LegacyCodeVariantCount = distributions.Length,
            DominantLeadCount = dominantLeadCount,
            DominantLeadPercentagePoints = dominantLeadPercentagePoints,
            ConflictSummary = conflictSummary,
            Distributions = distributions,
            Summary = $"recommended legacy code '{recommended.LegacyCode}' for {canonicalCode} ({recommended.Count}/{totalCount}, {(int)Math.Round(recommended.Share * 100d, MidpointRounding.AwayFromZero)}pp); {conflictSummary}"
        };
    }
}
