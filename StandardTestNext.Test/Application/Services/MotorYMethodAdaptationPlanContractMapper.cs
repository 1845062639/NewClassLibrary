using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYMethodAdaptationPlanContractMapper
{
    public static MotorYMethodAdaptationPlanContract Map(
        MotorYMethodDecisionSnapshot snapshot,
        Func<MotorYLegacyAlgorithmRoute?, MotorYBuildProfileContract?> profileMapper)
    {
        var selectedProfile = snapshot.ShouldPrioritizeDominantOverBaseline
            ? snapshot.DominantRoute
            : snapshot.BaselineRoute;
        var selectedCount = snapshot.ShouldPrioritizeDominantOverBaseline
            ? snapshot.DominantCount
            : snapshot.BaselineCount;
        var baselineShare = snapshot.BaselineShare;
        var selectedShare = snapshot.TotalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / snapshot.TotalCount, 4, MidpointRounding.AwayFromZero);
        var dominantLeadCount = Math.Max(0, snapshot.DominantCount - snapshot.BaselineCount);
        var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((snapshot.DominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
        var selectedLeadCountVsBaseline = Math.Max(0, selectedCount - snapshot.BaselineCount);
        var selectedLeadPercentagePointsVsBaseline = Math.Max(0, (int)Math.Round((selectedShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));

        return new MotorYMethodAdaptationPlanContract
        {
            CanonicalCode = snapshot.CanonicalCode,
            TotalCount = snapshot.TotalCount,
            BaselineProfile = profileMapper(snapshot.BaselineRoute),
            BaselineCount = snapshot.BaselineCount,
            BaselineShare = baselineShare,
            DominantProfile = profileMapper(snapshot.DominantRoute),
            DominantCount = snapshot.DominantCount,
            DominantShare = snapshot.DominantShare,
            SelectedProfile = profileMapper(selectedProfile),
            SelectedCount = selectedCount,
            SelectedShare = selectedShare,
            SelectionStrategy = snapshot.RecommendedStrategy,
            ShouldUseDominantRoute = snapshot.ShouldPrioritizeDominantOverBaseline,
            DominantOverrideThreshold = snapshot.DominantOverrideThreshold,
            DominantLeadCount = dominantLeadCount,
            DominantLeadPercentagePoints = dominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = selectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = selectedLeadPercentagePointsVsBaseline,
            SelectionReason = BuildSelectionReason(snapshot, selectedProfile, dominantLeadCount, dominantLeadPercentagePoints),
            AlgorithmEntry = selectedProfile?.LegacyAlgorithmEntry ?? string.Empty,
            SettingsMethodName = selectedProfile?.LegacySettingsMethodName ?? string.Empty,
            LegacyMethodName = selectedProfile?.LegacyMethodName ?? string.Empty,
            SelectedMethodSummary = BuildSelectedMethodSummary(snapshot, selectedProfile, selectedCount),
            BaselineDominantComparisonSummary = BuildBaselineDominantComparisonSummary(snapshot),
            Distributions = snapshot.Distributions
                .Select(MapDistribution)
                .ToArray()
        };
    }

    private static MotorYMethodDistributionContract MapDistribution(MotorYMethodDistributionSnapshot snapshot)
    {
        return new MotorYMethodDistributionContract
        {
            MethodValue = snapshot.MethodValue,
            Count = snapshot.Count,
            Share = snapshot.Share,
            Profile = MapBuildProfile(snapshot.Route)
        };
    }

    private static string BuildSelectedMethodSummary(
        MotorYMethodDecisionSnapshot snapshot,
        MotorYLegacyAlgorithmRoute? selectedProfile,
        int selectedCount)
    {
        var selectedMethod = selectedProfile?.MethodValue;
        var selectedMethodName = selectedProfile?.LegacyMethodName ?? snapshot.CanonicalCode;
        var selectedVariant = selectedProfile?.VariantKind ?? string.Empty;
        var share = snapshot.TotalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / snapshot.TotalCount, 4, MidpointRounding.AwayFromZero);

        return $"selected {selectedMethodName} method {selectedMethod} ({selectedVariant}) covering {selectedCount}/{snapshot.TotalCount} items ({share:P2})";
    }

    private static string BuildBaselineDominantComparisonSummary(MotorYMethodDecisionSnapshot snapshot)
    {
        var baselineMethod = snapshot.BaselineRoute?.MethodValue;
        var dominantMethod = snapshot.DominantRoute?.MethodValue;
        var baselineVariant = snapshot.BaselineRoute?.VariantKind ?? string.Empty;
        var dominantVariant = snapshot.DominantRoute?.VariantKind ?? string.Empty;
        var baselineShare = snapshot.TotalCount <= 0
            ? 0d
            : Math.Round((double)snapshot.BaselineCount / snapshot.TotalCount, 4, MidpointRounding.AwayFromZero);

        return $"baseline {baselineMethod} ({baselineVariant})={snapshot.BaselineCount}/{snapshot.TotalCount} ({baselineShare:P2}), dominant {dominantMethod} ({dominantVariant})={snapshot.DominantCount}/{snapshot.TotalCount} ({snapshot.DominantShare:P2})";
    }

    private static string BuildSelectionReason(
        MotorYMethodDecisionSnapshot snapshot,
        MotorYLegacyAlgorithmRoute? selectedProfile,
        int dominantLeadCount,
        int dominantLeadPercentagePoints)
    {
        var selectedMethod = selectedProfile?.MethodValue;
        var baselineMethod = snapshot.BaselineRoute?.MethodValue;
        var dominantMethod = snapshot.DominantRoute?.MethodValue;

        if (snapshot.ShouldPrioritizeDominantOverBaseline)
        {
            return $"selected dominant method {dominantMethod} over baseline {baselineMethod} because dominant share {snapshot.DominantShare:P2} reached threshold {snapshot.DominantOverrideThreshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)";
        }

        if (baselineMethod == dominantMethod)
        {
            return $"kept baseline method {selectedMethod} because baseline already matches dominant distribution ({snapshot.DominantShare:P2})";
        }

        return $"kept baseline method {baselineMethod} because dominant method {dominantMethod} share {snapshot.DominantShare:P2} did not reach threshold {snapshot.DominantOverrideThreshold:P0}";
    }

    private static MotorYBuildProfileContract? MapBuildProfile(MotorYLegacyAlgorithmRoute? route)
    {
        return route is null
            ? null
            : new MotorYBuildProfileContract
            {
                CanonicalCode = route.CanonicalCode,
                MethodValue = route.MethodValue,
                MethodKey = route.MethodKey,
                ProfileKey = route.ProfileKey,
                VariantKind = route.VariantKind,
                AlgorithmFamily = route.AlgorithmFamily,
                LegacyEnumName = route.LegacyEnumName,
                LegacyFormName = route.LegacyFormName,
                LegacyAlgorithmEntry = route.LegacyAlgorithmEntry,
                LegacyMethodName = route.LegacyMethodName,
                LegacySettingsMethodName = route.LegacySettingsMethodName,
                IsBaselineMethod = route.IsBaselineMethod
            };
    }
}
