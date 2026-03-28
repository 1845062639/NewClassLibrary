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

        var selection = CreateSelection(decision);
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

    public static MotorYMethodRouteSelectionSnapshot CreateSelection(MotorYMethodDecisionSnapshot snapshot)
    {
        var selectedRoute = snapshot.RecommendedRoute;
        var selectedCount = snapshot.ShouldPrioritizeDominantOverBaseline
            ? snapshot.DominantCount
            : snapshot.BaselineCount;
        var selectedShare = snapshot.TotalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / snapshot.TotalCount, 4, MidpointRounding.AwayFromZero);
        var selectedLeadCountVsBaseline = Math.Max(0, selectedCount - snapshot.BaselineCount);
        var selectedLeadPercentagePointsVsBaseline = Math.Max(0, (int)Math.Round((selectedShare - snapshot.BaselineShare) * 100d, MidpointRounding.AwayFromZero));

        return new MotorYMethodRouteSelectionSnapshot
        {
            CanonicalCode = snapshot.CanonicalCode,
            TotalCount = snapshot.TotalCount,
            BaselineRoute = snapshot.BaselineRoute,
            BaselineCount = snapshot.BaselineCount,
            BaselineShare = snapshot.BaselineShare,
            DominantRoute = snapshot.DominantRoute,
            DominantCount = snapshot.DominantCount,
            DominantShare = snapshot.DominantShare,
            SelectedRoute = selectedRoute,
            SelectedCount = selectedCount,
            SelectedShare = selectedShare,
            SelectionStrategy = snapshot.RecommendedStrategy,
            ShouldUseDominantRoute = snapshot.ShouldPrioritizeDominantOverBaseline,
            DominantOverrideThreshold = snapshot.DominantOverrideThreshold,
            DominantLeadCount = snapshot.DominantLeadCount,
            DominantLeadPercentagePoints = snapshot.DominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = selectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = selectedLeadPercentagePointsVsBaseline,
            SelectionReason = BuildSelectionReason(snapshot),
            SelectedMethodSummary = BuildSelectedMethodSummary(snapshot, selectedRoute, selectedCount, selectedShare),
            BaselineDominantComparisonSummary = BuildBaselineDominantComparisonSummary(snapshot),
            Distributions = snapshot.Distributions
        };
    }

    private static string BuildSelectedMethodSummary(
        MotorYMethodDecisionSnapshot snapshot,
        MotorYLegacyAlgorithmRoute? selectedRoute,
        int selectedCount,
        double selectedShare)
    {
        var selectedMethod = selectedRoute?.MethodValue;
        var selectedMethodName = selectedRoute?.LegacyMethodName ?? snapshot.CanonicalCode;
        var selectedVariant = selectedRoute?.VariantKind ?? string.Empty;
        return $"selected {selectedMethodName} method {selectedMethod} ({selectedVariant}) covering {selectedCount}/{snapshot.TotalCount} items ({selectedShare:P2})";
    }

    private static string BuildBaselineDominantComparisonSummary(MotorYMethodDecisionSnapshot snapshot)
    {
        var baselineMethod = snapshot.BaselineRoute?.MethodValue;
        var dominantMethod = snapshot.DominantRoute?.MethodValue;
        var baselineVariant = snapshot.BaselineRoute?.VariantKind ?? string.Empty;
        var dominantVariant = snapshot.DominantRoute?.VariantKind ?? string.Empty;
        return $"baseline {baselineMethod} ({baselineVariant})={snapshot.BaselineCount}/{snapshot.TotalCount} ({snapshot.BaselineShare:P2}), dominant {dominantMethod} ({dominantVariant})={snapshot.DominantCount}/{snapshot.TotalCount} ({snapshot.DominantShare:P2})";
    }

    private static string BuildSelectionReason(MotorYMethodDecisionSnapshot snapshot)
    {
        var baselineMethod = snapshot.BaselineRoute?.MethodValue;
        var dominantMethod = snapshot.DominantRoute?.MethodValue;
        var selectedMethod = snapshot.RecommendedRoute?.MethodValue;

        if (snapshot.ShouldPrioritizeDominantOverBaseline)
        {
            return $"selected dominant method {dominantMethod} over baseline {baselineMethod} because dominant share {snapshot.DominantShare:P2} reached threshold {snapshot.DominantOverrideThreshold:P0} (+{snapshot.DominantLeadCount} items, +{snapshot.DominantLeadPercentagePoints}pp)";
        }

        if (baselineMethod == dominantMethod)
        {
            return $"kept baseline method {selectedMethod} because baseline already matches dominant distribution ({snapshot.DominantShare:P2})";
        }

        return $"kept baseline method {baselineMethod} because dominant method {dominantMethod} share {snapshot.DominantShare:P2} did not reach threshold {snapshot.DominantOverrideThreshold:P0}";
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
