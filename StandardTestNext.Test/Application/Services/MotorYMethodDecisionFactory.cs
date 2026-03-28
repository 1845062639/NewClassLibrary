namespace StandardTestNext.Test.Application.Services;

internal static class MotorYMethodDecisionFactory
{
    public static MotorYMethodDecisionSnapshot Create(
        string canonicalCode,
        int totalCount,
        MotorYLegacyAlgorithmRoute? baselineRoute,
        int baselineCount,
        MotorYLegacyAlgorithmRoute? dominantRoute,
        int dominantCount,
        IReadOnlyList<MotorYMethodDistributionSnapshot> distributions,
        double dominantOverrideThreshold)
    {
        var dominantShare = totalCount <= 0
            ? 0d
            : Math.Round((double)dominantCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var baselineShare = totalCount <= 0
            ? 0d
            : Math.Round((double)baselineCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var dominantMethod = dominantRoute?.MethodValue;
        var baselineMethod = baselineRoute?.MethodValue;
        var dominantLeadCount = baselineMethod.HasValue && dominantMethod.HasValue && dominantMethod.Value != baselineMethod.Value
            ? Math.Max(0, dominantCount - baselineCount)
            : 0;
        var dominantLeadPercentagePoints = baselineMethod.HasValue && dominantMethod.HasValue && dominantMethod.Value != baselineMethod.Value
            ? Math.Max(0, (int)Math.Round((dominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero))
            : 0;
        var shouldPrioritizeDominant = baselineMethod.HasValue
            && dominantMethod.HasValue
            && dominantMethod.Value != baselineMethod.Value
            && dominantShare >= dominantOverrideThreshold;
        var recommendedRoute = shouldPrioritizeDominant
            ? dominantRoute
            : baselineRoute ?? dominantRoute;
        var recommendedStrategy = shouldPrioritizeDominant
            ? "dominant-threshold-over-baseline"
            : "baseline";

        var decision = new MotorYMethodDecisionSnapshot
        {
            CanonicalCode = canonicalCode,
            TotalCount = totalCount,
            BaselineRoute = baselineRoute,
            BaselineCount = baselineCount,
            DominantRoute = dominantRoute,
            DominantCount = dominantCount,
            RecommendedRoute = recommendedRoute,
            RecommendedStrategy = recommendedStrategy,
            ShouldPrioritizeDominantOverBaseline = shouldPrioritizeDominant,
            DominantShare = dominantShare,
            BaselineShare = baselineShare,
            DominantOverrideThreshold = dominantOverrideThreshold,
            DominantLeadCount = dominantLeadCount,
            DominantLeadPercentagePoints = dominantLeadPercentagePoints,
            RecommendationReason = BuildSelectionReason(
                shouldPrioritizeDominant,
                baselineMethod,
                dominantMethod,
                dominantShare,
                dominantOverrideThreshold,
                dominantLeadCount,
                dominantLeadPercentagePoints),
            Distributions = distributions
        };

        var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(decision);
        return new MotorYMethodDecisionSnapshot
        {
            CanonicalCode = decision.CanonicalCode,
            TotalCount = decision.TotalCount,
            BaselineRoute = decision.BaselineRoute,
            BaselineCount = decision.BaselineCount,
            DominantRoute = decision.DominantRoute,
            DominantCount = decision.DominantCount,
            RecommendedRoute = decision.RecommendedRoute,
            RecommendedStrategy = decision.RecommendedStrategy,
            ShouldPrioritizeDominantOverBaseline = decision.ShouldPrioritizeDominantOverBaseline,
            DominantShare = decision.DominantShare,
            BaselineShare = decision.BaselineShare,
            DominantOverrideThreshold = decision.DominantOverrideThreshold,
            DominantLeadCount = decision.DominantLeadCount,
            DominantLeadPercentagePoints = decision.DominantLeadPercentagePoints,
            RecommendationReason = decision.RecommendationReason,
            RecommendedMethodSummary = selection.SelectedMethodSummary,
            BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
            Distributions = decision.Distributions
        };
    }

    private static string BuildSelectionReason(
        bool shouldPrioritizeDominant,
        int? baselineMethod,
        int? dominantMethod,
        double dominantShare,
        double threshold,
        int dominantLeadCount,
        int dominantLeadPercentagePoints)
    {
        if (shouldPrioritizeDominant)
        {
            return $"selected dominant method {dominantMethod} over baseline {baselineMethod} because dominant share {dominantShare:P2} reached threshold {threshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)";
        }

        if (baselineMethod == dominantMethod)
        {
            return $"kept baseline method {baselineMethod} because baseline already matches dominant distribution ({dominantShare:P2})";
        }

        return $"kept baseline method {baselineMethod} because dominant method {dominantMethod} share {dominantShare:P2} did not reach threshold {threshold:P0}";
    }
}
