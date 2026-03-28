using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYMethodAdaptationPlanContractMapper
{
    public static MotorYMethodAdaptationPlanContract Map(
        MotorYMethodDecisionSnapshot snapshot,
        Func<MotorYLegacyAlgorithmRoute?, MotorYBuildProfileContract?> profileMapper)
    {
        var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(snapshot);
        var selectedProfile = selection.SelectedRoute;
        var dependencyProfile = MotorYLegacyAlgorithmDependencyCatalog.TryGet(selection.CanonicalCode);
        var requiredPayloadFields = dependencyProfile?.RequiredPayloadFields ?? Array.Empty<string>();
        var coverage = MotorYRequiredPayloadFieldCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            requiredPayloadFields,
            null);

        return new MotorYMethodAdaptationPlanContract
        {
            CanonicalCode = selection.CanonicalCode,
            TotalCount = selection.TotalCount,
            BaselineProfile = profileMapper(selection.BaselineRoute),
            BaselineCount = selection.BaselineCount,
            BaselineShare = selection.BaselineShare,
            DominantProfile = profileMapper(selection.DominantRoute),
            DominantCount = selection.DominantCount,
            DominantShare = selection.DominantShare,
            SelectedProfile = profileMapper(selectedProfile),
            SelectedCount = selection.SelectedCount,
            SelectedShare = selection.SelectedShare,
            SelectionStrategy = selection.SelectionStrategy,
            ShouldUseDominantRoute = selection.ShouldUseDominantRoute,
            DominantOverrideThreshold = selection.DominantOverrideThreshold,
            DominantLeadCount = selection.DominantLeadCount,
            DominantLeadPercentagePoints = selection.DominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = selection.SelectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = selection.SelectedLeadPercentagePointsVsBaseline,
            SelectionReason = selection.SelectionReason,
            AlgorithmEntry = selectedProfile?.LegacyAlgorithmEntry ?? string.Empty,
            SettingsMethodName = selectedProfile?.LegacySettingsMethodName ?? string.Empty,
            LegacyMethodName = selectedProfile?.LegacyMethodName ?? string.Empty,
            RequiresRatedParams = dependencyProfile?.RequiresRatedParams == true,
            UpstreamCanonicalCodes = dependencyProfile?.UpstreamCanonicalCodes ?? Array.Empty<string>(),
            RequiredPayloadFields = requiredPayloadFields,
            RequiredRatedParamFields = dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
            CoveredRequiredPayloadFieldCount = coverage.CoveredRequiredPayloadFieldCount,
            MissingRequiredPayloadFieldCount = coverage.MissingRequiredPayloadFieldCount,
            MissingRequiredPayloadFields = coverage.MissingRequiredPayloadFields,
            CoveredRequiredPayloadFields = coverage.CoveredRequiredPayloadFields,
            RequiredPayloadFieldCoverageRatio = coverage.RequiredPayloadFieldCoverageRatio,
            RequiredPayloadFieldCoveragePercentagePoints = coverage.RequiredPayloadFieldCoveragePercentagePoints,
            SamplePayloadAvailable = coverage.SamplePayloadAvailable,
            RequiredPayloadFieldCoverageSummary = coverage.RequiredPayloadFieldCoverageSummary,
            DependencyNotes = dependencyProfile?.Notes ?? string.Empty,
            SelectedMethodSummary = selection.SelectedMethodSummary,
            BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
            Distributions = selection.Distributions
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
